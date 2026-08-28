using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace SharpSCCM.UnitTests
{
    [TestClass]
    public class HelpersTests
    {
        [TestMethod]
        public void DecompressXMLNodes_TaskSequenceVariable_ExpandsVariableValue()
        {
            string taskSequenceXml = "<TaskSequence><variable name=\"OSDJoinPassword\">test-password</variable></TaskSequence>";
            byte[] taskSequenceBytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(taskSequenceXml)).ToArray();
            byte[] compressedBytes;

            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream compressor = new GZipStream(output, CompressionMode.Compress, true))
                {
                    compressor.Write(taskSequenceBytes, 0, taskSequenceBytes.Length);
                }
                compressedBytes = output.ToArray();
            }

            string compressedHex = BitConverter.ToString(compressedBytes).Replace("-", string.Empty);
            XmlDocument taskSequenceDoc = new XmlDocument();
            taskSequenceDoc.LoadXml($"<PolicyXML Compression=\"zlib\">{compressedHex}</PolicyXML>");

            Helpers.DecompressXMLNodes(taskSequenceDoc);

            XmlNode passwordNode = taskSequenceDoc.SelectSingleNode("//variable[@name='OSDJoinPassword']");
            Assert.IsNotNull(passwordNode);
            Assert.AreEqual("test-password", passwordNode.InnerText);
        }
    }
}
