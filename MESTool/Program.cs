using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;

namespace MESTool
{
    public class Program
    {
        const string TimeFormat = @"mm\:ss\.ffff";
        static string[] Table = new string[0xffff];
        static ushort[] InvTable = new ushort[0xffff];

        static List<int> TblLength = new List<int>();

        static void Main(string[] args)
        {
            string[] TblEntries = MESTool.Properties.Resources.CharTable.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string Entry in TblEntries)
            {
                string[] Parameters = Entry.Split(Convert.ToChar("="));
                Table[int.Parse(Parameters[0], NumberStyles.HexNumber)] = Parameters[1];
            }

            for (int i = 0; i < Table.Length; i++)
            {
                int OldLength = 0;
                for (int j = 0; j < Table.Length; j++)
                {
                    if (Table[j] != null && Table[j].Length > OldLength && !TblLength.Contains(Table[j].Length))
                        OldLength = Table[j].Length;
                }
                if (OldLength > 0) TblLength.Add(OldLength); else break;
            }

            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i] == null) continue;

                for (int j = 0; j < 0xffff; j++)
                {
                    if (Table[i] == Convert.ToChar(j).ToString())
                        InvTable[j] = (ushort)i;
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Bayonetta *.mes Text Dumper/Creator by gdkchan");
            Console.WriteLine("Version 0.2.2");
            Console.CursorTop++;
            Console.ResetColor();

            if (args.Length == 0)
            {
                PrintUsage();
            }
            else
            {
                string Operation = args[0];
                string FileName = null;

                if (args.Length == 2)
                {
                    FileName = args[1];
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Invalid number of parameters!");
                    Console.CursorTop++;
                    PrintUsage();
                    return;
                }

                switch (Operation)
                {
                    case "-d":
                        if (FileName == "-all")
                        {
                            string[] Files = Directory.GetFiles(Environment.CurrentDirectory);
                            foreach (string File in Files) if (Path.GetExtension(File).ToLower() == ".mes") Dump(File);
                        }
                        else
                            Dump(FileName);

                        break;
                    case "-c":
                        if (FileName == "-all")
                        {
                            string[] Folders = Directory.GetDirectories(Environment.CurrentDirectory);
                            foreach (string Folder in Folders) Create(Folder);
                        }
                        else
                            Create(FileName);

                        break;
                    case "-di":
                        if (FileName == "-all")
                        {
                            string[] Files = Directory.GetFiles(Environment.CurrentDirectory);
                            foreach (string File in Files) if (Path.GetExtension(File).ToLower() == ".idd") DumpIDD(File);
                        }
                        else
                            DumpIDD(FileName);

                        break;
                    case "-ci":
                        if (FileName == "-all")
                        {
                            string[] Folders = Directory.GetDirectories(Environment.CurrentDirectory);
                            foreach (string Folder in Folders) CreateIDD(Folder);
                        }
                        else
                            CreateIDD(FileName);

                        break;
                    case "-dt":
                        if (FileName == "-all")
                        {
                            string[] Files = Directory.GetFiles(Environment.CurrentDirectory);
                            foreach (string File in Files) if (Path.GetExtension(File).ToLower() == ".wtb") DumpWTB(File);
                        }
                        else
                            DumpWTB(FileName);

                        break;
                    case "-ct":
                        if (FileName == "-all")
                        {
                            string[] Files = Directory.GetFiles(Environment.CurrentDirectory);
                            foreach (string File in Files) if (Path.GetExtension(File).ToLower() == ".dds") CreateWTB(File);
                        }
                        else
                            CreateWTB(FileName);

                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: Invalid operation specified!");
                        Console.CursorTop++;
                        PrintUsage();
                        break;
                }
            }
        }

        static void PrintUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Usage:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("MESTool.exe [operation] [file|-all]");
            Console.CursorTop++;
            Console.WriteLine("[operation]");
            Console.WriteLine("-d  Dumps a *.mes file to a folder");
            Console.WriteLine("-c  Creates a *.mes file from a folder");
            Console.WriteLine("-di  Dumps a *.idd file to a folder");
            Console.WriteLine("-ci  Creates a *.idd file from a folder");
            Console.WriteLine("-dt  Dumps a *.wtb file to a *.dds texture");
            Console.WriteLine("-ct  Creates a *.wtb file from a *.dds texture");

            Console.CursorTop++;
            Console.WriteLine("-all  Manipulate all the files on the work directory");
            Console.CursorTop++;
            Console.WriteLine("Example:");
            Console.WriteLine("MESTool -d file.mes");
            Console.WriteLine("MESTool -d -all");
            Console.WriteLine("MESTool -c folder");
            Console.WriteLine("MESTool -c -all");
            Console.ResetColor();
        }

        private struct SectEntry
        {
            public uint Index;
            public uint Offset;
            public uint Length;
        }

        public class TextureMapUV
        {
            [XmlAttribute]
            public ushort Id;
            public float StartX;
            public float StartY;
            public float EndX;
            public float EndY;
        }

        public class TextureMapSize
        {
            [XmlAttribute]
            public ushort Id;
            public float Width;
            public float Height;
        }

        public class TextureMapUVSection
        {
            [XmlAttribute]
            public uint Count;
            public List<TextureMapUV> Entries;

            public TextureMapUVSection()
            {
                Entries = new List<TextureMapUV>();
            }
        }

        [XmlRootAttribute("TextureUV", Namespace = "gdkchan/MESTool")]
        public class TUV
        {
            public uint Count;
            public ushort Value;
            public List<TextureMapUV> Entries;

            public TUV()
            {
                Entries = new List<TextureMapUV>();
            }
        }

        public class TextureMapSizeSection
        {
            [XmlAttribute]
            public uint Count;
            public List<TextureMapSize> Entries;

            public TextureMapSizeSection()
            {
                Entries = new List<TextureMapSize>();
            }
        }

        [XmlRootAttribute("TextureMap", Namespace = "gdkchan/MESTool")]
        public class TextureMapInfo
        {
            public TextureMapUVSection UVTable;
            public TextureMapSizeSection SizeTable;

            public TextureMapInfo()
            {
                UVTable = new TextureMapUVSection();
                SizeTable = new TextureMapSizeSection();
            }
        }

        private static void Dump(string FileName)
        {
            FileStream Input = new FileStream(FileName, FileMode.Open);
            EndianBinaryReader Reader = new EndianBinaryReader(Input, EndianBinary.Endian.Big);

            string OutDir = Path.GetFileNameWithoutExtension(FileName);
            Directory.CreateDirectory(OutDir);

            uint TextureSectionOffset = Reader.ReadUInt32();
            uint TextCount = Reader.ReadUInt32();
            Reader.ReadInt32(); //-1

            /*
             * Texts
             */
            List<SectEntry> TextList = new List<SectEntry>();
            for (int i = 0; i < TextCount; i++)
            {
                SectEntry Entry = new SectEntry();

                Entry.Index = Reader.ReadUInt32();
                Entry.Offset = Reader.ReadUInt32() + 4;
                Entry.Length = Reader.ReadUInt32();

                TextList.Add(Entry);
            }

            StringBuilder Texts = new StringBuilder();
            foreach (SectEntry Entry in TextList)
            {
                Input.Seek(Entry.Offset, SeekOrigin.Begin);

                for (int i = 0; i < Entry.Length; i += 2)
                {
                    ushort Value = Reader.ReadUInt16();
                    string Character = Table[Value];

                    if (Character != null)
                        Texts.Append(Character);
                    else if (Value == 0x8000)
                        Texts.Append(Environment.NewLine);
                    else if (Value == 0x8f00)
                    {
                        Texts.Append("[id=" + Reader.ReadUInt16().ToString() + "]");
                        i += 2;
                    }
                    else if (Value == 0x8f01)
                    {
                        float Start = Reader.ReadUInt16() / 64f;
                        float End = Reader.ReadUInt16() / 64f;
                        TimeSpan StartPos = TimeSpan.FromSeconds(Start);
                        TimeSpan EndPos = TimeSpan.FromSeconds(End);
                        Texts.Append("[time=" + StartPos.ToString(TimeFormat) + "/" + EndPos.ToString(TimeFormat) + "]");
                        i += 4;
                    }
                    else
                        Texts.Append("[0x" + Value.ToString("X4") + "]");

                    if (i > Entry.Length) break;
                }

                Texts.Append(Environment.NewLine + Environment.NewLine);
            }
            File.WriteAllText(Path.Combine(OutDir, "Texts.txt"), Texts.ToString().TrimEnd());

            /*
             * Texture stuff
             */
            Input.Seek(TextureSectionOffset, SeekOrigin.Begin);
            uint Section2Offset = Reader.ReadUInt32() + TextureSectionOffset;
            uint BTWOffset = Reader.ReadUInt32() + TextureSectionOffset;

            TextureMapInfo TexInfo = new TextureMapInfo();
            uint Section1Count = Reader.ReadUInt32();
            TexInfo.UVTable.Count = Section1Count;
            for (int i = 0; i < Section1Count; i++)
            {
                TextureMapUV Entry = new TextureMapUV();
                Entry.Id = Reader.ReadUInt16();
                Reader.ReadUInt16();
                Entry.StartX = Reader.ReadSingle();
                Entry.StartY = Reader.ReadSingle();
                Entry.EndX = Reader.ReadSingle();
                Entry.EndY = Reader.ReadSingle();
                TexInfo.UVTable.Entries.Add(Entry);
            }

            Input.Seek(Section2Offset, SeekOrigin.Begin);
            uint Section2Count = Reader.ReadUInt32();
            TexInfo.SizeTable.Count = Section2Count;
            Input.Seek(0xc, SeekOrigin.Current);
            for (int i = 0; i < Section2Count; i++)
            {
                TextureMapSize Entry = new TextureMapSize();
                Entry.Id = Reader.ReadUInt16();
                Reader.ReadUInt16();
                Entry.Width = Reader.ReadSingle();
                Entry.Height = Reader.ReadSingle();
                TexInfo.SizeTable.Entries.Add(Entry);
                Reader.ReadUInt32();
            }

            FileStream TexInfoOut = new FileStream(Path.Combine(OutDir, "TextureMap.xml"), FileMode.Create);
            XmlSerializer Serializer = new XmlSerializer(typeof(TextureMapInfo));
            Serializer.Serialize(TexInfoOut, TexInfo);
            TexInfoOut.Close();

            Input.Seek(BTWOffset, SeekOrigin.Begin);
            DumpWTB(Reader, Path.Combine(OutDir, "Texture.dds"));

            Input.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dumped file " + Path.GetFileName(FileName) + "!");
            Console.ResetColor();
        }

        private struct IDDSection
        {
            public uint Index;
            public uint Offset;
        }

        private static void DumpIDD(string FileName)
        {
            FileStream Input = new FileStream(FileName, FileMode.Open);
            EndianBinaryReader Reader = new EndianBinaryReader(Input, EndianBinary.Endian.Big);

            string OutDir = Path.GetFileNameWithoutExtension(FileName);
            Directory.CreateDirectory(OutDir);

            IDDSection[] IDD = ParseIDDSection(Reader, 4);
            List<uint> PointerTable = new List<uint>();
            for (int i = 0; i < IDD.Length; i++) PointerTable.Add(IDD[i].Offset);
            PointerTable.Sort();
            PointerTable.Add((uint)Input.Length);

            Input.Seek(IDD[0].Offset, SeekOrigin.Begin);
            IDDSection[] Textures = ParseIDDSection(Reader);
            for (int i = 0; i < Textures.Length; i++)
            {
                Input.Seek(Textures[i].Offset, SeekOrigin.Begin);
                DumpWTB(Reader, Path.Combine(OutDir, "Texture_" + i + ".dds"));
            }

            Input.Seek(IDD[1].Offset, SeekOrigin.Begin);
            IDDSection[] TUVs = ParseIDDSection(Reader);
            for (int i = 0; i < TUVs.Length; i++)
            {
                Input.Seek(TUVs[i].Offset, SeekOrigin.Begin);

                TUV TexInfo = new TUV();
                Reader.ReadUInt32();
                TexInfo.Value = Reader.ReadUInt16();
                Reader.ReadUInt16();
                TexInfo.Count = Reader.ReadUInt16();
                Reader.ReadUInt16();
                for (int j = 0; j < TexInfo.Count; j++)
                {
                    TextureMapUV Entry = new TextureMapUV();
                    Entry.Id = Reader.ReadUInt16();
                    Reader.ReadUInt16();
                    Entry.StartX = Reader.ReadSingle();
                    Entry.StartY = Reader.ReadSingle();
                    Entry.EndX = Reader.ReadSingle();
                    Entry.EndY = Reader.ReadSingle();
                    TexInfo.Entries.Add(Entry);
                }

                FileStream TexInfoOut = new FileStream(Path.Combine(OutDir, "TextureMap_" + i + ".xml"), FileMode.Create);
                XmlSerializer Serializer = new XmlSerializer(typeof(TUV));
                Serializer.Serialize(TexInfoOut, TexInfo);
                TexInfoOut.Close();
            }

            Input.Seek(IDD[2].Offset, SeekOrigin.Begin);
            int IDTPointerIndex = PointerTable.IndexOf(IDD[2].Offset);
            uint IDTLength = PointerTable[IDTPointerIndex + 1] - PointerTable[IDTPointerIndex];
            byte[] IDTBuffer = new byte[IDTLength];
            Reader.Read(IDTBuffer, 0, IDTBuffer.Length);
            File.WriteAllBytes(Path.Combine(OutDir, "Section.idt"), IDTBuffer);

            Input.Seek(IDD[3].Offset, SeekOrigin.Begin);
            int MODPointerIndex = PointerTable.IndexOf(IDD[3].Offset);
            uint MODLength = PointerTable[MODPointerIndex + 1] - PointerTable[MODPointerIndex];
            byte[] MODBuffer = new byte[MODLength];
            Reader.Read(MODBuffer, 0, MODBuffer.Length);
            File.WriteAllBytes(Path.Combine(OutDir, "Section.mod"), MODBuffer);

            Input.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dumped file " + Path.GetFileName(FileName) + "!");
            Console.ResetColor();
        }

        private static IDDSection[] ParseIDDSection(EndianBinaryReader Reader, int Count = -1)
        {
            List<IDDSection> Sections = new List<IDDSection>();

            uint BaseOffset = (uint)Reader.BaseStream.Position;
            uint Signature = Reader.ReadUInt32();
            uint SectionCount = Reader.ReadUInt32();
            if (Count == -1) Count = (int)SectionCount;

            for (int i = 0; i < Count; i++)
            {
                IDDSection Entry = new IDDSection();

                Entry.Index = Reader.ReadUInt32();
                Entry.Offset = Reader.ReadUInt32() + BaseOffset;

                Sections.Add(Entry);
            }

            return Sections.ToArray();
        }

        private static void DumpWTB(string FileName)
        {
            FileStream Input = new FileStream(FileName, FileMode.Open);
            EndianBinaryReader Reader = new EndianBinaryReader(Input, EndianBinary.Endian.Big);

            DumpWTB(Reader, Path.GetFileNameWithoutExtension(FileName) + ".dds");
            Input.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dumped file " + Path.GetFileName(FileName) + "!");
            Console.ResetColor();
        }

        private static void DumpWTB(EndianBinaryReader Reader, string OutFileName)
        {
            uint WTBOffset = (uint)Reader.BaseStream.Position;
            uint WTBSignature = Reader.ReadUInt32();
            Reader.ReadUInt32();
            uint GTFCount = Reader.ReadUInt32();
            uint GTFPointerOffset = Reader.ReadUInt32() + WTBOffset;
            uint SectionLengthOffset = Reader.ReadUInt32() + WTBOffset;
            uint UnknowDataOffset = Reader.ReadUInt32() + WTBOffset;

            Reader.Seek(GTFPointerOffset, SeekOrigin.Begin);
            Reader.Seek(Reader.ReadUInt32() + WTBOffset, SeekOrigin.Begin);

            #region "Texture parsing and DDS generation"
            uint GTFVersion = Reader.ReadUInt32();
            uint GTFLength = Reader.ReadUInt32();
            uint GTFTextureCount = Reader.ReadUInt32();
            uint GTFId = Reader.ReadUInt32();
            uint GTFTextureDataOffset = Reader.ReadUInt32();
            uint GTFTextureDataLength = Reader.ReadUInt32();
            byte GTFTextureFormat = Reader.ReadByte();
            byte GTFMipmaps = Reader.ReadByte();
            byte GTFDimension = Reader.ReadByte();
            byte GTFCubemaps = Reader.ReadByte();
            uint GTFRemap = Reader.ReadUInt32();
            ushort GTFTextureWidth = Reader.ReadUInt16();
            ushort GTFTextureHeight = Reader.ReadUInt16();
            ushort GTFDepth = Reader.ReadUInt16();
            ushort GTFPitch = Reader.ReadUInt16();
            ushort GTFLocation = Reader.ReadUInt16();
            ushort GTFTextureOffset = Reader.ReadUInt16();
            Reader.Seek(8, SeekOrigin.Current);

            bool isSwizzle = (GTFTextureFormat & 0x20) == 0;
            bool isNormalized = (GTFTextureFormat & 0x40) == 0;
            GTFTextureFormat = (byte)(GTFTextureFormat & ~0x60);

            byte[] TextureData = new byte[GTFTextureDataLength];
            Reader.Read(TextureData, 0, TextureData.Length);

            FileStream DDSOut = new FileStream(OutFileName, FileMode.Create);
            BinaryWriter DDS = new BinaryWriter(DDSOut);

            DDS.Write(0x20534444); //DDS Signature
            DDS.Write((uint)0x7c); //Header size (without the signature)
            DDS.Write((uint)0x00021007); //DDS Flags
            DDS.Write((uint)GTFTextureHeight);
            DDS.Write((uint)GTFTextureWidth);
            DDS.Write((uint)GTFPitch);
            DDS.Write((uint)GTFDepth);
            DDS.Write((uint)GTFMipmaps);
            DDSOut.Seek(0x2c, SeekOrigin.Current); //Reserved space for future use
            DDS.Write((uint)0x20); //PixelFormat structure size (32 bytes)

            uint PixelFlags = 0;
            if (GTFTextureFormat >= 0x86 && GTFTextureFormat <= 0x88) PixelFlags = 4; //Is DXT Compressed
            else PixelFlags = 0x40; //Isn't compressed
            DDS.Write(PixelFlags);

            switch (GTFTextureFormat)
            {
                case 0x86: DDS.Write(Encoding.ASCII.GetBytes("DXT1")); break;
                case 0x87: DDS.Write(Encoding.ASCII.GetBytes("DXT3")); break;
                case 0x88: DDS.Write(Encoding.ASCII.GetBytes("DXT5")); break;
                default:
                    DDS.Write((uint)0);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: Unsupported GTF format!");
                    Console.ResetColor();
                    break;
            }
            DDSOut.Seek(20, SeekOrigin.Current);

            DDS.Write((uint)0x400000); //Caps 1
            if (GTFCubemaps > 0) DDS.Write((uint)0x200); else DDS.Write((uint)0); //Caps 2
            DDS.Write((uint)0); //Unused stuff
            DDS.Write((uint)0);
            DDS.Write((uint)0);
            DDS.Write(TextureData);
            DDS.Close();
            #endregion
        }

        private static void Create(string Folder)
        {
            string TextsFile = Path.Combine(Folder, "Texts.txt");
            string TexMapFile = Path.Combine(Folder, "TextureMap.xml");
            string TextureFile = Path.Combine(Folder, "Texture.dds");

            if (File.Exists(TextsFile))
            {
                string[] Texts = File.ReadAllText(TextsFile).Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);

                FileStream Output = new FileStream(Folder + ".mes", FileMode.Create);
                EndianBinaryWriter Writer = new EndianBinaryWriter(Output, EndianBinary.Endian.Big);

                MemoryStream TextsBlock = new MemoryStream();
                EndianBinaryWriter TextsBlockWriter = new EndianBinaryWriter(TextsBlock, EndianBinary.Endian.Big);

                Writer.Write((uint)0);
                Writer.Write((uint)Texts.Length);
                Writer.Write(-1);

                uint Index = 0;
                foreach (string Text in Texts)
                {
                    uint StartPosition = (uint)TextsBlock.Position;

                    for (int i = 0; i < Text.Length; i++)
                    {
                        if (i + 4 <= Text.Length && Text.Substring(i, 4) == "[id=" && Text.Substring(i + 4).IndexOf("]") > -1)
                        {
                            int StartPos = i + 4;
                            int Length = Text.Substring(StartPos).IndexOf("]");
                            uint Id = uint.Parse(Text.Substring(StartPos, Length));
                            TextsBlockWriter.Write((ushort)0x8f00);
                            TextsBlockWriter.Write((ushort)Id);
                            i += Length + 4;
                        }
                        else if (i + 6 <= Text.Length && Text.Substring(i, 6) == "[time=" && Text.Substring(i + 6).IndexOf("]") > -1)
                        {
                            int StartPos = i + 6;
                            int Length = Text.Substring(StartPos).IndexOf("]");
                            string[] Contents = Text.Substring(StartPos, Length).Split(Convert.ToChar("/"));
                            TimeSpan StartTime = TimeSpan.ParseExact(Contents[0], TimeFormat, CultureInfo.InvariantCulture);
                            TimeSpan EndTime = TimeSpan.ParseExact(Contents[1], TimeFormat, CultureInfo.InvariantCulture);
                            TextsBlockWriter.Write((ushort)0x8f01);
                            TextsBlockWriter.Write((ushort)(StartTime.TotalSeconds * 64f));
                            TextsBlockWriter.Write((ushort)(EndTime.TotalSeconds * 64f));
                            i += Length + 6;
                        }
                        else if (i + 3 <= Text.Length && Text.Substring(i, 3) == "[0x" && Text.Substring(i + 3).IndexOf("]") > -1)
                        {
                            int StartPos = i + 3;
                            int Length = Text.Substring(StartPos).IndexOf("]");
                            string Hex = Text.Substring(StartPos, Length);
                            TextsBlockWriter.Write(ushort.Parse(Hex, NumberStyles.HexNumber));
                            i += Length + 3;
                        }
                        else if (i + 2 <= Text.Length && Text.Substring(i, 2) == Environment.NewLine)
                        {
                            TextsBlockWriter.Write((ushort)0x8000);
                            i++;
                        }
                        else
                        {
                            string Character = Text.Substring(i, 1);

                            //Optimized for max speed
                            if (Character != "[") //CHANGE THIS if you ever add a more than one character String in table that doesn't start with "["
                            {
                                ushort CC = Convert.ToChar(Character);
                                ushort InvCC = InvTable[CC];
                                if (InvCC != 0) TextsBlockWriter.Write(InvCC);
                            }
                            else
                            {
                                for (int j = 0; j < TblLength.Count; j++)
                                {
                                    if (i + TblLength[j] > Text.Length) continue;
                                    string TestString = Text.Substring(i, TblLength[j]);

                                    for (int k = 0; k < Table.Length; k++)
                                    {
                                        if (Table[k] == null) continue;
                                        if (TestString == Table[k])
                                        {
                                            TextsBlockWriter.Write((ushort)k);
                                            i += TblLength[j] - 1;
                                            j = TblLength.Count;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Writer.Write(Index++);
                    Writer.Write((uint)((StartPosition + 0xc + Texts.Length * 0xc) - 4));
                    Writer.Write((uint)(TextsBlock.Position - StartPosition));
                }

                Writer.Write(TextsBlock.ToArray());
                TextsBlock.Close();

                while ((Output.Position & 0xfff) != 0) Writer.Write((byte)0);
                uint TextureSectionOffset = (uint)Output.Position;
                Output.Seek(0, SeekOrigin.Begin);
                Writer.Write(TextureSectionOffset);
                Output.Seek(TextureSectionOffset, SeekOrigin.Begin);

                if (File.Exists(TexMapFile))
                {
                    FileStream TexInfoIn = new FileStream(TexMapFile, FileMode.Open);
                    XmlSerializer Deserializer = new XmlSerializer(typeof(TextureMapInfo));
                    TextureMapInfo TexInfo = (TextureMapInfo)Deserializer.Deserialize(TexInfoIn);
                    TexInfoIn.Close();

                    Writer.Write(12 + TexInfo.UVTable.Count * 20);
                    Writer.Write((uint)0);

                    Writer.Write(TexInfo.UVTable.Count);
                    for (int i = 0; i < TexInfo.UVTable.Count; i++)
                    {
                        Writer.Write(TexInfo.UVTable.Entries[i].Id);
                        Writer.Write((ushort)0);
                        Writer.Write(TexInfo.UVTable.Entries[i].StartX);
                        Writer.Write(TexInfo.UVTable.Entries[i].StartY);
                        Writer.Write(TexInfo.UVTable.Entries[i].EndX);
                        Writer.Write(TexInfo.UVTable.Entries[i].EndY);
                    }

                    Writer.Write(TexInfo.SizeTable.Count);
                    Writer.Write((uint)0x24);
                    Writer.Write((uint)0x24);
                    Writer.Write((uint)0);
                    for (int i = 0; i < TexInfo.SizeTable.Count; i++)
                    {
                        Writer.Write(TexInfo.SizeTable.Entries[i].Id);
                        Writer.Write((ushort)0);
                        Writer.Write(TexInfo.SizeTable.Entries[i].Width);
                        Writer.Write(TexInfo.SizeTable.Entries[i].Height);
                        Writer.Write((uint)0x1000000);
                    }

                    while ((Output.Position & 0xfff) != 0) Writer.Write((byte)0);
                    long WTBOffset = Output.Position;
                    Output.Seek(TextureSectionOffset + 4, SeekOrigin.Begin);
                    Writer.Write((uint)(WTBOffset - TextureSectionOffset));

                    if (File.Exists(TextureFile))
                    {
                        Output.Seek(WTBOffset, SeekOrigin.Begin);
                        CreateWTB(Writer, TextureFile);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Warning: \"Texture.dds\" not found on folder " + Path.GetFileName(Folder) + "!");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: \"TextureMap.xml\" not found on folder " + Path.GetFileName(Folder) + "!");
                    Console.ResetColor();
                }

                Output.Seek(Output.Length, SeekOrigin.Begin);
                while ((Output.Position & 0x7ff) != 0) Writer.Write((byte)0);
                Output.Close();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Created file " + Path.GetFileName(Folder) + ".mes!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: \"Texts.txt\" not found on folder " + Path.GetFileName(Folder) + "!");
                Console.ResetColor();
            }
        }

        private static void CreateIDD(string Folder)
        {
            FileStream Output = new FileStream(Folder + ".idd", FileMode.Create);
            EndianBinaryWriter Writer = new EndianBinaryWriter(Output, EndianBinary.Endian.Big);
            
            uint TextureCount = 0;
            uint TextureMapCount = 0;
            int i = 0, j = 0;
            for (;;) if (File.Exists(Path.Combine(Folder, "Texture_" + i++ + ".dds"))) TextureCount++; else break;
            for (;;) if (File.Exists(Path.Combine(Folder, "TextureMap_" + j++ + ".xml"))) TextureMapCount++; else break;

            MemoryStream TEXSection = new MemoryStream();
            EndianBinaryWriter TEXSectionWriter = new EndianBinaryWriter(TEXSection, EndianBinary.Endian.Big);
            TEXSectionWriter.Write((uint)0x54455800); //TEX Signature
            TEXSectionWriter.Write(TextureCount);
            long TEXHeaderPosition = 8;
            long TEXDataPosition = TEXHeaderPosition + (TextureCount * 8);
            while ((TEXDataPosition & 0xf) != 0) TEXDataPosition++;
            uint TEXBaseDataOffset = (uint)TEXDataPosition;
            for (i = 0; i < TextureCount; i++)
            {
                string FileName = Path.Combine(Folder, "Texture_" + i + ".dds");

                TEXSection.Seek(TEXHeaderPosition, SeekOrigin.Begin);
                TEXSectionWriter.Write((uint)i);
                TEXSectionWriter.Write((uint)TEXDataPosition);
                TEXHeaderPosition = TEXSection.Position;

                TEXSection.Seek(TEXDataPosition, SeekOrigin.Begin);
                CreateWTB(TEXSectionWriter, FileName, TEXBaseDataOffset);
                TEXDataPosition = TEXSection.Position;
            }

            MemoryStream TUVSection = new MemoryStream();
            EndianBinaryWriter TUVSectionWriter = new EndianBinaryWriter(TUVSection, EndianBinary.Endian.Big);
            TUVSectionWriter.Write((uint)0x54555600); //TUV Signature
            TUVSectionWriter.Write(TextureMapCount);
            long TUVHeaderPosition = 8;
            long TUVDataPosition = TUVHeaderPosition + (TextureCount * 8);
            while ((TUVDataPosition & 0xf) != 0) TUVDataPosition++;
            for (i = 0; i < TextureCount; i++)
            {
                FileStream TexInfoIn = new FileStream(Path.Combine(Folder, "TextureMap_" + i + ".xml"), FileMode.Open);
                XmlSerializer Deserializer = new XmlSerializer(typeof(TUV));
                TUV TextureInfo = (TUV)Deserializer.Deserialize(TexInfoIn);
                TexInfoIn.Close();

                TUVSection.Seek(TUVHeaderPosition, SeekOrigin.Begin);
                TUVSectionWriter.Write((uint)i);
                TUVSectionWriter.Write((uint)TUVDataPosition);
                TUVHeaderPosition = TUVSection.Position;

                TUVSection.Seek(TUVDataPosition, SeekOrigin.Begin);
                TUVSectionWriter.Write((uint)1);
                TUVSectionWriter.Write(TextureInfo.Value);
                TUVSectionWriter.Write((ushort)0);
                TUVSectionWriter.Write((ushort)TextureInfo.Count);
                TUVSectionWriter.Write((ushort)8);
                for (j = 0; j < TextureInfo.Count; j++)
                {
                    TUVSectionWriter.Write(TextureInfo.Entries[j].Id);
                    TUVSectionWriter.Write((ushort)0);
                    TUVSectionWriter.Write(TextureInfo.Entries[j].StartX);
                    TUVSectionWriter.Write(TextureInfo.Entries[j].StartY);
                    TUVSectionWriter.Write(TextureInfo.Entries[j].EndX);
                    TUVSectionWriter.Write(TextureInfo.Entries[j].EndY);
                }
                while ((TUVSection.Position & 0xf) != 0) TUVSectionWriter.Write((byte)0);
                TUVDataPosition = TUVSection.Position;
            }

            Writer.Write((uint)0x49444400); //IDD Signature
            Writer.Write((uint)3); //Section count

            Output.Seek(0x30, SeekOrigin.Begin);
            if (File.Exists(Path.Combine(Folder, "Section.idt")))
            {
                byte[] Buff = File.ReadAllBytes(Path.Combine(Folder, "Section.idt"));
                Writer.Write(Buff);
            }
            else
            {
                Writer.Write((uint)0x49445400); //IDT Signature
                Writer.Write((uint)0); //Section count (0 = No data)
            }
            while ((Output.Position & 0xf) != 0) Writer.Write((byte)0);

            uint TUVSectionOffset = (uint)Output.Position;
            Writer.Write(TUVSection.ToArray());
            TUVSection.Close();

            while (((Output.Position + TEXHeaderPosition) & 0xfff) != 0) Writer.Write((byte)0);
            uint TEXSectionOffset = (uint)Output.Position;
            Writer.Write(TEXSection.ToArray());
            TEXSection.Close();

            while ((Output.Position & 0xf) != 0) Writer.Write((byte)0);
            uint MODSectionOffset = (uint)Output.Position;
            if (File.Exists(Path.Combine(Folder, "Section.mod")))
            {
                byte[] Buff = File.ReadAllBytes(Path.Combine(Folder, "Section.mod"));
                Writer.Write(Buff);
            }
            else
            {
                Writer.Write((uint)0x4d4f4400); //MOD Signature
                Writer.Write((uint)0); //Section count (0 = No data)
            }

            Output.Seek(8, SeekOrigin.Begin);
            Writer.Write((uint)0);
            Writer.Write(TEXSectionOffset);
            Writer.Write((uint)1);
            Writer.Write(TUVSectionOffset);
            Writer.Write((uint)2);
            Writer.Write((uint)0x30);
            Writer.Write((uint)3);
            Writer.Write(MODSectionOffset);

            Output.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Created file " + Path.GetFileName(Folder) + ".idd!");
            Console.ResetColor();
        }

        private static void CreateWTB(string FileName)
        {
            FileStream Output = new FileStream(Path.GetFileNameWithoutExtension(FileName) + ".wtb", FileMode.Create);
            EndianBinaryWriter Writer = new EndianBinaryWriter(Output, EndianBinary.Endian.Big);

            CreateWTB(Writer, FileName);

            Output.Close();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Created file " + Path.GetFileNameWithoutExtension(FileName) + ".wtb!");
            Console.ResetColor();
        }

        private static void CreateWTB(EndianBinaryWriter Writer, string TextureFile, uint BaseOffset = 0)
        {
            uint WTBOffset = (uint)Writer.BaseStream.Position;
            Writer.Write((uint)0x425457);
            Writer.Write((uint)0);
            Writer.Write((uint)1);
            Writer.Write((uint)0x20);
            Writer.Write((uint)0x40);
            Writer.Write((uint)0x60);
            Writer.Seek(WTBOffset + 0x20, SeekOrigin.Begin);
            Writer.Write((uint)0xcc);
            Writer.Seek(WTBOffset + 0x60, SeekOrigin.Begin);
            Writer.Write((uint)0x40000000);

            FileStream DDSIn = new FileStream(TextureFile, FileMode.Open);
            BinaryReader DDS = new BinaryReader(DDSIn);

            uint DDSLength = (uint)(DDSIn.Length - 0x80);
            uint DDSPaddedLength = DDSLength;
            while ((DDSPaddedLength & 0x7f) != 0) DDSPaddedLength++;
            Writer.Seek(WTBOffset + 0xcc, SeekOrigin.Begin);
            Writer.Write((uint)0x1040100);
            Writer.Write(DDSPaddedLength);
            Writer.Write((uint)1);
            Writer.Write((uint)0);
            Writer.Write((uint)0x80);
            Writer.Write(DDSLength);

            DDSIn.Seek(0xc, SeekOrigin.Begin);
            uint Height = DDS.ReadUInt32();
            uint Width = DDS.ReadUInt32();
            uint Pitch = DDS.ReadUInt32();
            uint Depth = DDS.ReadUInt32();
            uint Mipmaps = DDS.ReadUInt32();

            DDSIn.Seek(0x54, SeekOrigin.Begin);
            byte[] FCC = new byte[4];
            DDS.Read(FCC, 0, FCC.Length);
            string FourCC = Encoding.ASCII.GetString(FCC);
            switch (FourCC)
            {
                case "DXT1": Writer.Write((byte)0x86); break;
                case "DXT3": Writer.Write((byte)0x87); break;
                case "DXT5": Writer.Write((byte)0x88); break;
                default:
                    Writer.Write((byte)0);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: Unsupported DDS format!");
                    Console.ResetColor();
                    break;
            }
            Writer.Write((byte)Mipmaps);
            Writer.Write((byte)2);
            Writer.Write((byte)0);
            Writer.Write((uint)0xAAE4);
            Writer.Write((ushort)Width);
            Writer.Write((ushort)Height);
            Writer.Write((ushort)1);
            Writer.Seek(0xe, SeekOrigin.Current);

            DDSIn.Seek(0x80, SeekOrigin.Begin);
            byte[] TextureData = new byte[DDSIn.Length - 0x80];
            DDS.Read(TextureData, 0, TextureData.Length);
            Writer.Write(TextureData);
            while (((Writer.BaseStream.Position - BaseOffset) & 0x7f) != 0) Writer.Write((byte)0xee);
    
            DDSIn.Close();

            uint Length = (uint)(Writer.BaseStream.Position - WTBOffset);
            Writer.Seek(WTBOffset + 0x40, SeekOrigin.Begin);
            Writer.Write(Length);
        }
    }
}
