using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RLE
{
    internal class RLE
    {

        // ---------------------- Compress ---------------------------
        public void CompressFile(string dataFileName, string archFileName)
        {
            byte[] data = File.ReadAllBytes(dataFileName);
            byte[] arch = CompressBytes(data);
            File.WriteAllBytes(archFileName, arch);
        }

        private byte[] CompressBytes(byte[] data)
        {
            List<List<int>> groupBytes = GroupingBytes(data);
            byte[] arch = Compress(groupBytes);
            return arch;
        }

        private byte[] Compress(List<List<int>> groupBytes)
        {
            List<byte> arch = new List<byte>();
            List<byte> TypeZero = new List<byte>();
            foreach(List<int> group in groupBytes)
            {
                if(group.Count > 1)
                {
                    int countGroups;
                    if(TypeZero.Count > 0)
                        TypeZeroClear();

                    countGroups = group.Count / 128;
                    byte symbol = (byte)group.First();
                    while(countGroups-- != 0)
                    {
                        arch.Add(1 << 7 | 127);
                        arch.Add(symbol);
                    }

                    int otherGroups = group.Count % 128;
                    if(otherGroups == 1)
                        TypeZero.Add(symbol);
                    else if(otherGroups >= 2)
                    {
                        arch.Add((byte)(1 << 7 | (otherGroups - 2)));
                        arch.Add(symbol);
                    }
                }
                else if(group.Count == 1) 
                    TypeZero.Add((byte)group.First());
            }
            if(TypeZero.Count > 0)
                TypeZeroClear();
            return arch.ToArray();


            void TypeZeroClear()
            {
                int countGroups = TypeZero.Count / 128;
                while (countGroups-- != 0)
                {
                    arch.Add(0 << 7 | 127);
                    for (int i = 0; i < 128; ++i)
                    {
                        arch.Add(TypeZero.First());
                        TypeZero.RemoveAt(0);
                    }
                }
                if (TypeZero.Count % 128 > 0)
                {
                    arch.Add((byte)(0 << 7 | (TypeZero.Count - 1)));
                    while (TypeZero.Count > 0)
                    {
                        arch.Add(TypeZero.First());
                        TypeZero.RemoveAt(0);
                    }
                }
            }
        }

        private List<List<int>> GroupingBytes(byte[] data)
        {
            List<List<int>> listBytes = new List<List<int>>();
            List<int> saveList = new List<int>();
            int saveLastBit = -1;
            foreach(byte bit in data)
            {
                if(saveLastBit != (int)bit)
                {
                    listBytes.Add(saveList);
                    saveList = new List<int>();

                    saveLastBit = (int)bit;
                }
                saveList.Add((int)bit);
            }
            listBytes.Add(saveList);
            listBytes.RemoveAt(0);
            return listBytes;
        }

        // ---------------------- Decompress ---------------------------
        public void DecompressFile(string archFileName, string dataFileName)
        {
            byte[] arch = File.ReadAllBytes(archFileName);
            byte[] data = DecompressBytes(arch);
            File.WriteAllBytes(dataFileName, data);
        }

        private byte[] DecompressBytes(byte[] arch)
        {
            List<byte> data = new List<byte>();
            for(int i = 0; i < arch.Length; ++i)
            {
                int type = (arch[i] & 128) >> 7;
                int count = arch[i] & 127;
                if (type == 0)
                {
                    count += 1;
                    while (count-- > 0)
                        data.Add(arch[++i]);
                }
                else
                {
                    i++;
                    count += 2;
                    while (count-- > 0)
                        data.Add(arch[i]);
                }
                
            }
            return data.ToArray();
        }
    }
}
