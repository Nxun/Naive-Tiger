using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScintillaNet;
using System.IO;
using Antlr.Runtime;

namespace NaiveTiger
{
    public partial class Form1 : Form
    {
        Marker _marker;

        Graphics _g;
        private int _marginWidth = 26;
        private int _maxLineLength;
        private string _filePath;
        private string FilePath
        {
            set
            {
                _filePath = value;
                UpdateTitle();
            }
            get
            {
                return _filePath;
            }
        }


        public Form1()
        {
            InitializeComponent();
            listView1.SmallImageList = new ImageList();
            listView1.SmallImageList.Images.Add(global::NaiveTiger.Properties.Resources.ImageError);

            _g = this.CreateGraphics();
            InitializeScintilla();
            FilePath = string.Empty;
        }

        private void InitializeScintilla()
        {
            _marker = scintilla1.Markers[0];
            _marker.Symbol = MarkerSymbol.Background;
            _marker.BackColor = Color.FromArgb(255, 225, 225);

            scintilla1.Indentation.SmartIndentType = SmartIndent.Simple;

            scintilla1.Lexing.StreamCommentPrefix = "/*";
            scintilla1.Lexing.StreamCommentSufix = "*/";
            scintilla1.Lexing.LineCommentPrefix = "//";

            Font font = new Font("Consolas", 10);
            scintilla1.Font = font;

            scintilla1.Styles[0].ForeColor = Color.Black; // keyword
            scintilla1.Styles[0].Font = font;

            scintilla1.Styles[1].ForeColor = Color.Blue; // keyword
            scintilla1.Styles[1].Font = font;

            scintilla1.Styles[2].ForeColor = Color.DarkOrange; // int
            scintilla1.Styles[2].Font = font;

            scintilla1.Styles[3].ForeColor = Color.FromArgb(0xA31515); // string
            scintilla1.Styles[3].Font = font;

            scintilla1.Styles[4].ForeColor = Color.Green; // comment
            scintilla1.Styles[4].Font = font;


        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.UndoRedo.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.UndoRedo.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Clipboard.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Clipboard.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Clipboard.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Selection.SelectAll();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.FindReplace.ShowFind();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.FindReplace.ShowReplace();
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.GoTo.ShowGoToDialog();
        }

        private void makeUpperCaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Commands.Execute(BindableCommand.UpperCase);
        }

        private void makeLowerCaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Commands.Execute(BindableCommand.LowerCase);
        }

        private void commentBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Commands.Execute(BindableCommand.StreamComment);
        }

        private void commentLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Commands.Execute(BindableCommand.LineComment);
        }

        private void uncommentLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Commands.Execute(BindableCommand.LineUncomment);
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Zoom++;
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Zoom--;
        }

        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla1.Zoom = 0;
        }

        private void toolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = !toolStrip1.Visible;
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = !statusStrip1.Visible;
        }

        private void lineNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lineNumberToolStripMenuItem.Checked)
            {
                scintilla1.Margins.Margin0.Width = _marginWidth;
            }
            else
            {
                scintilla1.Margins.Margin0.Width = 0;
            }
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.Clipboard.Cut();
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.Clipboard.Copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.Clipboard.Paste();
        }

        private void undoToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.UndoRedo.Undo();
        }

        private void redoToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.UndoRedo.Redo();
        }

        private void zoomInToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.Zoom++;
        }

        private void zoomOutToolStripButton_Click(object sender, EventArgs e)
        {
            scintilla1.Zoom--;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        private void NewFile()
        {
            if (!UnsavePrompt())
                return;
            FilePath = string.Empty;
            scintilla1.ResetText();
            scintilla1.UndoRedo.EmptyUndoBuffer();
            scintilla1.Modified = false;
            splitContainer1.Panel2Collapsed = true;
        }

        private void UpdateTitle()
        {
            string file;
            if (FilePath.Length == 0)
            {
                file = "Untitled";
            }
            else
            {
                int k = FilePath.LastIndexOf('\\');
                file = k > 0 ? FilePath.Substring(k + 1) : FilePath;
            }
            if (scintilla1.Modified)
            {
                Text = string.Format("{0}* - {1}", file, Program.Title);
            }
            else
            {
                Text = string.Format("{0} - {1}", file, Program.Title);
            }
        }

        private bool UnsavePrompt()
        {
            if (scintilla1.Modified)
            {
                string message = "File has changed. Do you want to save the changes?";
                DialogResult dr = MessageBox.Show(message, Program.Title, MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Yes)
                {
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        return SaveAs();
                    }
                    else
                    {
                        return Save(FilePath);
                    }
                }
                else if (dr == DialogResult.No)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void OpenFile(string file)
        {
            if (!UnsavePrompt())
                return;
            FilePath = file;
            scintilla1.Text = File.ReadAllText(FilePath);
            scintilla1.UndoRedo.EmptyUndoBuffer();
            scintilla1.Modified = false;
            splitContainer1.Panel2Collapsed = true;
        }

        private void OpenFile()
        {
            if (!UnsavePrompt())
                return;
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            FilePath = openFileDialog1.FileName;
            scintilla1.Text = File.ReadAllText(FilePath);
            scintilla1.UndoRedo.EmptyUndoBuffer();
            scintilla1.Modified = false;
            splitContainer1.Panel2Collapsed = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                SaveAs();
            }
            else
            {
                Save(FilePath);
            }
        }

        private bool SaveAs()
        {
            saveFileDialog1.FileName = string.Empty;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FilePath = saveFileDialog1.FileName;
                return Save(FilePath);
            }
            return false;
        }

        private bool Save(string filePath)
        {
            using (FileStream fs = File.Create(filePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
                bw.Write(scintilla1.RawText, 0, scintilla1.RawText.Length - 1);

            scintilla1.Modified = false;
            return true;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                SaveAs();
            }
            else
            {
                Save(FilePath);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!UnsavePrompt())
            {
                e.Cancel = true;
            }
        }

        private void scintilla1_TextChanged(object sender, EventArgs e)
        {
            if (_maxLineLength != scintilla1.Lines.Count.ToString().Length)
            {
                _maxLineLength = scintilla1.Lines.Count.ToString().Length;
                UpdateMarginWidth();
            }
            if (scintilla1.TextLength == 0)
                return;

            scintilla1.Lines.Current.DeleteMarker(_marker);
            //scintilla1.Styles.ClearDocumentStyle();
            
            ANTLRStringStream stream = new ANTLRStringStream(scintilla1.Text);
            Lexer1 lexer = new Lexer1(stream);
            IToken token;
            IToken last = null;
            while ((token = lexer.NextToken()).Type != -1)
            {
                last = token;
                switch (token.Type)
                {
                    case 4: // Block Comment
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(4);
                        break;
                    case 7: // Line Comment
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(4);
                        break;
                    case 9: // Reserved Word
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(1);
                        break;
                    case 5: // Identifier
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(0);
                        break;
                    case 6: // Int
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(2);
                        break;
                    case 10: // String
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(3);
                        break;
                    case 8: // Other
                        scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(0);
                        break;
                    case 11: // Whitespace
                        //scintilla1.GetRange(token.StartIndex, token.StopIndex + 1).SetStyle(0);
                        break;
                    default:
                        MessageBox.Show("Unknown token type");
                        break;
                }
            }

            int mark = 0;
            if (last == null)
                mark = 0;
            else if (last.StopIndex + 1 < scintilla1.TextLength)
                mark = last.StopIndex + 1;
            else
                return;

            while (mark < scintilla1.TextLength && scintilla1.CharAt(mark) != '\"' && scintilla1.CharAt(mark) != '/')
            {
                ++mark;
            }
            if (mark >= scintilla1.TextLength)
                return;
            else if (scintilla1.CharAt(mark) == '\"')
            {
                scintilla1.GetRange(mark, scintilla1.TextLength).SetStyle(3);
            }
            else
            {
                scintilla1.GetRange(mark, scintilla1.TextLength).SetStyle(4);
            }
        }

        private void scintilla1_ModifiedChanged(object sender, EventArgs e)
        {
            if (scintilla1.Modified)
            {
                UpdateTitle();
            }
            else
            {
                UpdateTitle();
            }
        }

        private void scintilla1_ZoomChanged(object sender, EventArgs e)
        {
            UpdateMarginWidth();
        }

        private void UpdateMarginWidth()
        {
            try
            {
                int len = 8 + _g.MeasureString(scintilla1.Lines.Count.ToString(), new Font("微软雅黑", 9 + scintilla1.Zoom)).ToSize().Width;
                _marginWidth = Math.Max(len, 26);
                if (lineNumberToolStripMenuItem.Checked)
                    scintilla1.Margins.Margin0.Width = _marginWidth;
            }
            catch { }
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void compileToolStripButton_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void Compile()
        {
            toolStripStatusLabel1.Text = "Compiling...";
            compileToolStripButton.Enabled = false;
            compileToolStripMenuItem.Enabled = false;

            string output;
            if (Path.HasExtension(FilePath))
                output = Path.ChangeExtension(FilePath, "s");
            else
                output = FilePath + ".s";
            NaiveTigerCompiler.Compiler compiler = new NaiveTigerCompiler.Compiler(scintilla1.Text, output);
            backgroundWorker1.RunWorkerAsync(compiler);
        }


        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int line = (listView1.SelectedItems[0].Tag as NaiveTigerCompiler.Error).Pos.Line;
            int column = (listView1.SelectedItems[0].Tag as NaiveTigerCompiler.Error).Pos.Column;
            GotoPos(line, column);
        }

        private void GotoPos(int line, int column)
        {
            scintilla1.GoTo.Position(scintilla1.Lines[line - 1].StartPosition + column);
            scintilla1.Focus();
        }

        private void MarkLine(int line)
        {
            scintilla1.Lines[line - 1].AddMarker(_marker);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Cancel = !(e.Argument as NaiveTigerCompiler.Compiler).Compile();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            scintilla1.Markers.DeleteAll(_marker);
            if (!e.Cancelled)
            {
                toolStripStatusLabel1.Text = "Compile Successful";
                splitContainer1.Panel2Collapsed = true;
            }
            else
            {
                toolStripStatusLabel1.Text = "Compile Failed";
                splitContainer1.Panel2Collapsed = false;
                listView1.Items.Clear();
                foreach (NaiveTigerCompiler.Error error in NaiveTigerCompiler.Compiler.ErrorList)
                {
                    ListViewItem item = new ListViewItem(string.Empty, 0);
                    item.SubItems.Add(error.Pos.Line == 0 ? string.Empty : error.Pos.Line.ToString());
                    item.SubItems.Add(error.Pos.Line == 0 ? string.Empty : error.Pos.Column.ToString());
                    item.SubItems.Add(error.Message);
                    item.Tag = error;
                    listView1.Items.Add(item);
                    if (error.Pos.Line > 0)
                        scintilla1.Lines[error.Pos.Line - 1].AddMarker(_marker);
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            compileToolStripButton.Enabled = true;
            compileToolStripMenuItem.Enabled = true;
        }

        private void scintilla1_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("drag drop");
        }

        private void scintilla1_FileDrop(object sender, FileDropEventArgs e)
        {
            OpenFile(e.FileNames[0]);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox1 about = new AboutBox1())
            {
                about.ShowDialog();
            }
        }

    }
}
