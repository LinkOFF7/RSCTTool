using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace rsct
{
    internal class RSCT
    {
        private struct Header
        {
            public string Magic;
            public int Zero;
            public int StringsCount;
            public int OffsetTableStartOffset;
            public int StringsStartOffset;
        }

        uint[] MessageIds, MessageOffsets;
        string[] Strings;

        private Header ReadHeader(ref BinaryReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Header header = new Header();
            header.Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (header.Magic != "RSCT")
                throw new FormatException($"Incorrect magic - {header.Magic}");
            header.Zero = reader.ReadInt32();
            header.StringsCount = reader.ReadInt32();
            header.OffsetTableStartOffset = reader.ReadInt32();
            header.StringsStartOffset = reader.ReadInt32();
            return header;
        }

        public void ConvertToRSCT(string input)
        {
            string[] newText = File.ReadAllLines(input);
            MessageIds = new uint[newText.Length];
            MessageOffsets = new uint[newText.Length];
            Strings = new string[newText.Length];

            for(int i = 0; i < newText.Length; i++)
            {
                MessageIds[i] = Convert.ToUInt32(newText[i].Split(new string[] {"[@]"}, StringSplitOptions.None)[0]);
                Strings[i] = newText[i].Split(new string[] {"[@]"}, StringSplitOptions.None)[1];
            }

            using (FileStream fs = File.Create(Path.GetFileNameWithoutExtension(input)))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                writer.Write(Encoding.ASCII.GetBytes("RSCT"));
                writer.Write(new int());
                writer.Write(Strings.Length);
                writer.Write(0x14);

                int stringsStartOffset = (int)writer.BaseStream.Position + (Strings.Length * 8) + 4;
                writer.Write(stringsStartOffset);

                writer.BaseStream.Position = stringsStartOffset;
                for(int i = 0; i < newText.Length; i++)
                {
                    MessageOffsets[i] = (uint)writer.BaseStream.Position;
                    string newLine = Strings[i].Insert(Strings[i].Length, "\0").Replace("{CRLF}", "\r\n").Replace("{CR}", "\r").Replace("{LF}", "\n");
                    writer.Write(newLine.Length * 2);
                    writer.Write(Encoding.Unicode.GetBytes(newLine));
                }

                writer.BaseStream.Position = 0x14;
                for (int i = 0; i < newText.Length; i++)
                {
                    writer.Write(MessageIds[i]);
                    writer.Write(MessageOffsets[i]);
                }
            }
        }

        public void Extract(string input)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(input));
            Header header = ReadHeader(ref reader);
            Console.WriteLine("Processing: {0}", input);
            MessageIds = new uint[header.StringsCount];
            MessageOffsets = new uint[header.StringsCount];
            Strings = new string[header.StringsCount];

            reader.BaseStream.Position = header.OffsetTableStartOffset;
            for(int i = 0; i < header.StringsCount; i++)
            {
                MessageIds[i] = reader.ReadUInt32();
                MessageOffsets[i] = reader.ReadUInt32();
            }

            if(reader.BaseStream.Position != header.StringsStartOffset)
            {
                Console.WriteLine("Reader position doesn't match header value: {0}", reader.BaseStream.Position);
                Console.ReadKey();
            }

            for (int i = 0; i < header.StringsCount; i++)
            {
                reader.BaseStream.Position = MessageOffsets[i];
                int len = reader.ReadInt32();
                Strings[i] = MessageIds[i].ToString() + "[@]" + Encoding.Unicode.GetString(reader.ReadBytes(len)).Replace("\0", "").Replace("\r\n", "{CRLF}").Replace("\r", "{CR}").Replace("\n", "{LF}");
            }
            File.WriteAllLines(input + ".txt", Strings);
        }
    }
}
