using System;
using System.IO;

namespace Synchronizer
{
    public class Synchronize
    {

        /*MIT License

        Copyright(c) 2020 Sipos Attila

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files(the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.*/

        public void SyncAllFiles(string source, string dest, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(source);
            DirectoryInfo desDir = new DirectoryInfo(dest);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                     "Source directory does not exist or could not be found: "
                     + source);
            }

            DirectoryInfo[] directories = dir.GetDirectories();

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dest, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in directories)
                {
                    string temppath = Path.Combine(dest, subdir.Name);
                    SyncAllFiles(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public void SyncDelete(string sourcePath, string sourceFilePath, string destPath) 
        {
            sourcePath = sourcePath.Remove(0, 3);
            sourcePath = sourcePath.Replace("\\", " ");
            string[] source = sourcePath.Split();
            destPath = destPath.Remove(0, 3);
            destPath = destPath.Replace("\\", " ");
            string[] dest = destPath.Split();

            string fileOrDirToDelete = sourceFilePath.Replace(source[source.Length - 1], dest[dest.Length - 1]);

            try
            {
                File.Delete(fileOrDirToDelete);
            }
            catch
            {
                Directory.Delete(fileOrDirToDelete);
            }
        }
    }
}
