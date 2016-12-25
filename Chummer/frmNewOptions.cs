﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Options;
using Chummer.Classes;
using Chummer.Datastructures;
using Chummer.UI.Options;
using Chummer.UI.Options.ControlGenerators;

namespace Chummer
{
	public partial class frmNewOptions : Form
	{
	    private Control _currentVisibleControl;
	    private AbstractOptionTree _winformTree;
	    private SimpleTree<OptionItem> _rawTree;
	    private List<IOptionWinFromControlFactory> _controlFactories;
	    public frmNewOptions()
		{
			InitializeComponent();



		    this.Load += OnLoad;


		}

	    private void OnLoad(object sender, EventArgs eventArgs)
	    {
	        _controlFactories = new List<IOptionWinFromControlFactory>()
	        {
	            new CheckBoxOptionFactory(),
	            new NumericUpDownOptionFactory()
	        };

	        //TODO: dropdown that allows you to select/add multiple
	        CharacterOptions o = Program.OptionsManager.Default;

	        OptionExtactor extactor = new OptionExtactor(
	            new List<Predicate<OptionItem>>(
	                _controlFactories.Select
	                    <IOptionWinFromControlFactory, Predicate<OptionItem>>
	                    (x => x.IsSupported)));


	        _rawTree = extactor.Extract(o);
	        _winformTree = GenerateWinFormTree(_rawTree);
	        _winformTree.Children.Add(new BookNode(new HashSet<string>(){"SR5"}));


	        PopulateTree(treeView1.Nodes, _winformTree);

	        if (treeView1.SelectedNode == null) {treeView1.SelectedNode = treeView1.Nodes[0];}

	        MaybeSpawnAndMakeVisible(treeView1.SelectedNode);
	    }

	    private AbstractOptionTree GenerateWinFormTree(SimpleTree<OptionItem> tree)
	    {
	        SimpleOptionTree so = new SimpleOptionTree(tree.Tag.ToString(), new List<OptionRenderItem>(tree.Leafs), _controlFactories);
	        so.Children.AddRange(tree.Children.Select(GenerateWinFormTree));
	        return so;
	    }

	    private void MaybeSpawnAndMakeVisible(TreeNode selectedNode)
	    {
	        AbstractOptionTree tree = (AbstractOptionTree) selectedNode.Tag;
	        if (_currentVisibleControl != null) _currentVisibleControl.Visible = false;

	        if (!tree.Created)
	        {
	            Control c = tree.ControlLazy();
	            Controls.Add(c);
                c.Location = new Point(treeView1.Right+8, 8);
		        c.Height = treeView1.Height;
		        c.Width = treeView1.Parent.Width - treeView1.Width - 36;
	            c.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
	        }

	      
           _currentVisibleControl = tree.ControlLazy();
           _currentVisibleControl.Visible = true;
        }

	    private void PopulateTree(TreeNodeCollection collection, AbstractOptionTree tree)
	    {
	        foreach (AbstractOptionTree child in tree.Children)
	        {
	            TreeNode n = collection.Add(child.Name); //TODO: Should probably hit LanguageManager
	            n.Tag = child;
                PopulateTree(n.Nodes, child);
	        }
	    }

	    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MaybeSpawnAndMakeVisible(e.Node);
        }
    }
}
