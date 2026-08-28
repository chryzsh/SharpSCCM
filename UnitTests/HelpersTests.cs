using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SharpSCCM.UnitTests
{
    [TestClass]
    public class MgmtPointMessagingTests
    {
        [TestMethod]
        public void ExtractTaskSequenceVariables_CompressedTaskSequence_ExtractsVariableValue()
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
            string taskSequencePolicy = $"<PolicyXML Compression=\"zlib\">{compressedHex}</PolicyXML>";

            string extractedVariables = MgmtPointMessaging.ExtractTaskSequenceVariables(taskSequencePolicy);

            StringAssert.Contains(extractedVariables, "TaskSequence variable 'OSDJoinPassword': test-password");
        }
    }
}
