﻿#region Arx One FTP
// Arx One FTP
// A simple FTP client
// https://github.com/ArxOne/FTP
// Released under MIT license http://opensource.org/licenses/MIT
#endregion

namespace ArxOne.FtpTest.Platform
{
    using System;
    using System.Linq;
    using Ftp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class PlatformTest
    {
        public static void SpaceNameTest(string platform)
        {
            NameTest(platform, "A and B", "C and D");
        }

        public static void BracketNameTest(string platform)
        {
            NameTest(platform, "X[]Y", "Z{}[]T");
        }

        public static void ParenthesisNameTest(string platform)
        {
            NameTest(platform, "i()j", "k()l");
        }

        private static void NameTest(string platform, string folderName, string childName, string protocol = "ftp")
        {
            if(string.Equals(platform,"FileZilla",StringComparison.InvariantCultureIgnoreCase))
                Assert.Inconclusive("FileZilla does not support escaping for special names (and yes, this is a shame)");
            var testHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(testHost.Uri, testHost.Credential))
            {
                var folder = (ftpClient.ServerType == FtpServerType.Windows ? "/" : "/tmp/") + folderName;
                var file = folder + "/" + childName;
                try
                {
                    ftpClient.Mkd(folder);
                    using (var s = ftpClient.Stor(file))
                        s.WriteByte(123);

                    var c = ftpClient.ListEntries(folder).SingleOrDefault();
                    Assert.IsNotNull(c);
                    Assert.AreEqual(childName, c.Name);
                    var c2 = ftpClient.StatEntries(folder).SingleOrDefault();
                    Assert.IsNotNull(c2);
                    Assert.AreEqual(childName, c2.Name);

                    using (var r = ftpClient.Retr(file))
                    {
                        Assert.AreEqual(123, r.ReadByte());
                        Assert.AreEqual(-1, r.ReadByte());
                    }
                }
                finally
                {
                    ftpClient.Dele(file);
                    ftpClient.Rmd(folder);
                }
            }
        }

        public static void ListTest(string platform, bool passive, string protocol = "ftp")
        {
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { Passive = passive }))
            {
                var list = ftpClient.ListEntries("/");
                if (ftpClient.ServerType == FtpServerType.Unix)
                    Assert.IsTrue(list.Any(e => e.Name == "tmp"));
            }
        }
    }
}
