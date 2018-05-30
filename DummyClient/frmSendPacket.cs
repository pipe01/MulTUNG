using Microsoft.CSharp;
using MulTUNG;
using MulTUNG.Packets;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace DummyClient
{
    public partial class frmSendPacket : Form
    {
        public frmSendPacket()
        {
            InitializeComponent();

            txtCode.Text = LastCode;
        }

        private static string LastCode = @"new Packet
{
    
}";

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                int lineIndex = txtCode.GetLineFromCharIndex(txtCode.SelectionStart);
                string line = txtCode.Lines[lineIndex];
                int caretOnLine = txtCode.SelectionStart - txtCode.GetFirstCharIndexOfCurrentLine();
                string lineUpToCaret = line.Substring(0, caretOnLine);

                if (string.IsNullOrWhiteSpace(lineUpToCaret))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;

                    int indentation = lineUpToCaret.Length / 4;

                    string newIndent = indentation > 1 ? new string(' ', (indentation - 1) * 4) : "";

                    string[] newLines = new string[txtCode.Lines.Length];
                    Array.Copy(txtCode.Lines, 0, newLines, 0, txtCode.Lines.Length);

                    newLines[lineIndex] = newIndent + line.TrimStart();

                    int caretPos = txtCode.SelectionStart;

                    txtCode.Lines = newLines;

                    txtCode.SelectionStart = caretPos - 4;
                }
            }
            else if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtCode_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.IsInputKey = true;

                int sel = txtCode.SelectionStart;

                txtCode.Text = txtCode.Text.Insert(sel, "    ");

                txtCode.SelectionStart = sel + 4;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string source = @"using MulTUNG.Packeting.Packets;
using UnityEngine;

public class Container
{
    public static Packet Create()
    {
        return " + txtCode.Text + @";
    }
}";

            Dictionary<string, string> providerOptions = new Dictionary<string, string>
                {
                    {"CompilerVersion", "v3.5"}
                };
            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            compilerParams.ReferencedAssemblies.Add(typeof(Packet).Assembly.Location);
            compilerParams.ReferencedAssemblies.Add(typeof(UnityEngine.Vector3).Assembly.Location);

            CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, source);

            if (results.Errors.Count != 0)
                throw new Exception("Mission failed!");

            object o = results.CompiledAssembly.CreateInstance("Container");
            MethodInfo mi = o.GetType().GetMethod("Create");
            var packet = mi.Invoke(o, null);

            LastCode = txtCode.Text;

            Network.SendPacket((Packet)packet);
        }
    }
}
