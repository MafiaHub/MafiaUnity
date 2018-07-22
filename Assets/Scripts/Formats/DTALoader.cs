using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public class DTALoader
{
    public DTALoader(uint key1, uint key2)
    {
        this.key1 = key1;
        this.key2 = key2;
    }

    public struct FileHeader
    {
        public uint fileCount;
        public uint fileTableOffset;
        public uint fileTableSize;
        public uint unknown;
    }

    unsafe public struct FileTableRecord
    {
        public uint extra1;
        public uint headerOffset;            // where DataFileHeader starts
        public uint dataOffset;              // where the data start
        public fixed byte nameHint[16];            //[16] file name, often incomplete
    }

    public unsafe struct DataFileHeader
    {
        public uint unknown1;
        public uint unknown2;
        public uint unknown3;
        public uint unknown4;
        public uint fileSize;
        public uint compressedBlockCount;
        public byte nameLength;
        public fixed byte flags[7];
        //public byte[] name; //[256]
        //We will read name from name lenght :)
    }

    [Flags]
    public enum BlockType : uint
    {
        Uncompressed = 0,
        LZSS_RLE = 1,
        DPCM0 = 8,
        DPCM1 = 12,
        DPCM2 = 16,
        DPCM3 = 20,
        DPCM4 = 24,
        DPCM5 = 28,
        DPCM6 = 32
    }

    public unsafe struct WavHeader
    {
        public uint chunkId;
        public uint chunkSize;
        public uint type;
        public uint chunkId2;
        public uint chunkSize2;
        public ushort tags;
        public ushort channels;
        public uint samplesPerSecond;
        public uint bytesPerSecond;
        public ushort blockAlign;
        public ushort sampleBits;
        public uint chunkId3;
        public uint chunkSize3;
    }

    private WavHeader wavHeader;

    private ushort[] WavDeltas = {
        0x0000,0x0001,0x0002,0x0004,0x0008,0x000c,0x0012,0x0018,0x0020,0x0029,0x0032,0x003d,
        0x0049,0x0055,0x0063,0x0072,0x0082,0x0092,0x00a4,0x00b7,0x00cb,0x00df,0x00f5,0x010c,
        0x0124,0x013d,0x0157,0x0172,0x018e,0x01ab,0x01c9,0x01e8,0x0208,0x0229,0x024b,0x026e,
        0x0292,0x02b7,0x02dd,0x0304,0x032c,0x0355,0x037f,0x03ab,0x03d7,0x0404,0x0432,0x0461,
        0x0492,0x04c3,0x04f5,0x0529,0x055d,0x0592,0x05c9,0x0600,0x0638,0x0672,0x06ac,0x06e8,
        0x0724,0x0761,0x07a0,0x07df,0x0820,0x0861,0x08a4,0x08e7,0x092c,0x0972,0x09b8,0x0a00,
        0x0a48,0x0a92,0x0add,0x0b28,0x0b75,0x0bc3,0x0c12,0x0c61,0x0cb2,0x0d04,0x0d57,0x0daa,
        0x0dff,0x0e55,0x0eac,0x0f04,0x0f5d,0x0fb7,0x1012,0x106d,0x10ca,0x1128,0x1187,0x11e7,
        0x1248,0x12aa,0x130d,0x1371,0x13d7,0x143d,0x14a4,0x150c,0x1575,0x15df,0x164a,0x16b7,
        0x1724,0x1792,0x1801,0x1871,0x18e3,0x1955,0x19c8,0x1a3d,0x1ab2,0x1b28,0x1ba0,0x1c18,
        0x1c91,0x1d0c,0x1d87,0x1e04,0x1e81,0x1f00,0x1f7f,0x2000,0x0000,0x0001,0x0003,0x0006,
        0x000c,0x0013,0x001b,0x0025,0x0030,0x003d,0x004c,0x005c,0x006d,0x0080,0x0095,0x00ab,
        0x00c3,0x00dc,0x00f6,0x0113,0x0130,0x014f,0x0170,0x0193,0x01b6,0x01dc,0x0203,0x022b,
        0x0255,0x0280,0x02ad,0x02dc,0x030c,0x033d,0x0370,0x03a5,0x03db,0x0412,0x044c,0x0486,
        0x04c2,0x0500,0x053f,0x0580,0x05c2,0x0606,0x064c,0x0692,0x06db,0x0725,0x0770,0x07bd,
        0x080c,0x085c,0x08ad,0x0900,0x0955,0x09ab,0x0a02,0x0a5c,0x0ab6,0x0b12,0x0b70,0x0bcf,
        0x0c30,0x0c92,0x0cf6,0x0d5b,0x0dc2,0x0e2b,0x0e95,0x0f00,0x0f6d,0x0fdb,0x104b,0x10bd,
        0x1130,0x11a5,0x121b,0x1292,0x130b,0x1386,0x1402,0x1480,0x14ff,0x1580,0x1602,0x1686,
        0x170b,0x1792,0x181b,0x18a4,0x1930,0x19bd,0x1a4b,0x1adb,0x1b6d,0x1c00,0x1c94,0x1d2a,
        0x1dc2,0x1e5b,0x1ef6,0x1f92,0x2030,0x20cf,0x2170,0x2212,0x22b6,0x235b,0x2402,0x24aa,
        0x2554,0x2600,0x26ad,0x275b,0x280b,0x28bd,0x2970,0x2a24,0x2ada,0x2b92,0x2c4b,0x2d06,
        0x2dc2,0x2e80,0x2f3f,0x3000,0x0000,0x0002,0x0004,0x0009,0x0010,0x0019,0x0024,0x0031,
        0x0041,0x0052,0x0065,0x007a,0x0092,0x00ab,0x00c7,0x00e4,0x0104,0x0125,0x0149,0x016e,
        0x0196,0x01bf,0x01eb,0x0219,0x0249,0x027a,0x02ae,0x02e4,0x031c,0x0356,0x0392,0x03d0,
        0x0410,0x0452,0x0496,0x04dc,0x0524,0x056e,0x05ba,0x0609,0x0659,0x06ab,0x06ff,0x0756,
        0x07ae,0x0809,0x0865,0x08c3,0x0924,0x0986,0x09eb,0x0a52,0x0aba,0x0b25,0x0b92,0x0c00,
        0x0c71,0x0ce4,0x0d59,0x0dd0,0x0e48,0x0ec3,0x0f40,0x0fbf,0x1040,0x10c3,0x1148,0x11cf,
        0x1259,0x12e4,0x1371,0x1400,0x1491,0x1525,0x15ba,0x1651,0x16eb,0x1786,0x1824,0x18c3,
        0x1965,0x1a08,0x1aae,0x1b55,0x1bff,0x1cab,0x1d58,0x1e08,0x1eba,0x1f6e,0x2024,0x20db,
        0x2195,0x2251,0x230f,0x23cf,0x2491,0x2555,0x261b,0x26e3,0x27ae,0x287a,0x2948,0x2a18,
        0x2aeb,0x2bbf,0x2c95,0x2d6e,0x2e48,0x2f24,0x3003,0x30e3,0x31c6,0x32aa,0x3391,0x347a,
        0x3564,0x3651,0x3740,0x3830,0x3923,0x3a18,0x3b0f,0x3c08,0x3d03,0x3e00,0x3eff,0x4000,
        0x0000,0x0002,0x0005,0x000b,0x0014,0x001f,0x002d,0x003e,0x0051,0x0066,0x007e,0x0099,
        0x00b6,0x00d6,0x00f8,0x011d,0x0145,0x016e,0x019b,0x01ca,0x01fb,0x022f,0x0266,0x029f,
        0x02db,0x0319,0x035a,0x039d,0x03e3,0x042b,0x0476,0x04c4,0x0514,0x0566,0x05bb,0x0613,
        0x066d,0x06ca,0x0729,0x078b,0x07ef,0x0856,0x08bf,0x092b,0x099a,0x0a0b,0x0a7e,0x0af4,
        0x0b6d,0x0be8,0x0c66,0x0ce6,0x0d69,0x0dee,0x0e76,0x0f01,0x0f8d,0x101d,0x10af,0x1144,
        0x11db,0x1274,0x1310,0x13af,0x1450,0x14f4,0x159b,0x1643,0x16ef,0x179d,0x184d,0x1900,
        0x19b6,0x1a6e,0x1b29,0x1be6,0x1ca6,0x1d68,0x1e2d,0x1ef4,0x1fbe,0x208a,0x2159,0x222b,
        0x22ff,0x23d6,0x24af,0x258a,0x2669,0x2749,0x282d,0x2912,0x29fb,0x2ae6,0x2bd3,0x2cc3,
        0x2db6,0x2eab,0x2fa2,0x309c,0x3199,0x3298,0x339a,0x349e,0x35a5,0x36af,0x37bb,0x38c9,
        0x39da,0x3aee,0x3c04,0x3d1c,0x3e37,0x3f55,0x4075,0x4198,0x42bd,0x43e5,0x4510,0x463d,
        0x476c,0x489e,0x49d3,0x4b0a,0x4c43,0x4d80,0x4ebe,0x5000,0x0000,0x0002,0x0006,0x000d,
        0x0018,0x0026,0x0036,0x004a,0x0061,0x007b,0x0098,0x00b8,0x00db,0x0101,0x012a,0x0156,
        0x0186,0x01b8,0x01ed,0x0226,0x0261,0x029f,0x02e1,0x0326,0x036d,0x03b8,0x0406,0x0456,
        0x04aa,0x0501,0x055b,0x05b8,0x0618,0x067b,0x06e1,0x074a,0x07b6,0x0825,0x0898,0x090d,
        0x0985,0x0a01,0x0a7f,0x0b01,0x0b85,0x0c0d,0x0c98,0x0d25,0x0db6,0x0e4a,0x0ee1,0x0f7b,
        0x1018,0x10b8,0x115b,0x1201,0x12aa,0x1356,0x1405,0x14b8,0x156d,0x1625,0x16e1,0x179f,
        0x1861,0x1925,0x19ed,0x1ab7,0x1b85,0x1c56,0x1d2a,0x1e01,0x1eda,0x1fb7,0x2097,0x217a,
        0x2260,0x234a,0x2436,0x2525,0x2617,0x270d,0x2805,0x2900,0x29ff,0x2b00,0x2c05,0x2d0c,
        0x2e17,0x2f25,0x3036,0x3149,0x3260,0x337a,0x3497,0x35b7,0x36da,0x3800,0x3929,0x3a55,
        0x3b85,0x3cb7,0x3dec,0x3f25,0x4060,0x419e,0x42e0,0x4425,0x456c,0x46b7,0x4804,0x4955,
        0x4aa9,0x4c00,0x4d5a,0x4eb7,0x5017,0x517a,0x52e0,0x5449,0x55b5,0x5724,0x5896,0x5a0c,
        0x5b84,0x5d00,0x5e7e,0x6000,0x0000,0x0002,0x0007,0x000f,0x001c,0x002c,0x003f,0x0057,
        0x0071,0x008f,0x00b1,0x00d7,0x00ff,0x012c,0x015c,0x018f,0x01c7,0x0201,0x023f,0x0281,
        0x02c7,0x030f,0x035c,0x03ac,0x03ff,0x0457,0x04b1,0x050f,0x0571,0x05d7,0x063f,0x06ac,
        0x071c,0x078f,0x0806,0x0881,0x08ff,0x0981,0x0a06,0x0a8f,0x0b1c,0x0bac,0x0c3f,0x0cd6,
        0x0d71,0x0e0f,0x0eb1,0x0f56,0x0fff,0x10ac,0x115c,0x120f,0x12c6,0x1381,0x143f,0x1501,
        0x15c6,0x168f,0x175c,0x182c,0x18ff,0x19d6,0x1ab1,0x1b8f,0x1c71,0x1d56,0x1e3f,0x1f2b,
        0x201b,0x210f,0x2206,0x2301,0x23ff,0x2501,0x2606,0x270f,0x281b,0x292b,0x2a3f,0x2b56,
        0x2c71,0x2d8f,0x2eb1,0x2fd6,0x30ff,0x322b,0x335b,0x348f,0x35c6,0x3700,0x383f,0x3980,
        0x3ac6,0x3c0f,0x3d5b,0x3eab,0x3ffe,0x4156,0x42b0,0x440e,0x4570,0x46d5,0x483e,0x49ab,
        0x4b1b,0x4c8e,0x4e05,0x4f80,0x50fe,0x5280,0x5405,0x558e,0x571b,0x58ab,0x5a3e,0x5bd5,
        0x5d70,0x5f0e,0x60b0,0x6255,0x63fe,0x65aa,0x675a,0x690e,0x6ac5,0x6c80,0x6e3e,0x7000,
        0x0000,0x0002,0x0008,0x0012,0x0020,0x0032,0x0049,0x0063,0x0082,0x00a4,0x00cb,0x00f5,
        0x0124,0x0157,0x018e,0x01c9,0x0208,0x024b,0x0292,0x02dd,0x032c,0x037f,0x03d7,0x0432,
        0x0492,0x04f5,0x055d,0x05c9,0x0638,0x06ac,0x0724,0x07a0,0x0820,0x08a4,0x092c,0x09b8,
        0x0a48,0x0add,0x0b75,0x0c11,0x0cb2,0x0d57,0x0dff,0x0eac,0x0f5d,0x1011,0x10ca,0x1187,
        0x1248,0x130d,0x13d6,0x14a4,0x1575,0x164a,0x1724,0x1801,0x18e2,0x19c8,0x1ab2,0x1b9f,
        0x1c91,0x1d87,0x1e81,0x1f7f,0x2081,0x2187,0x2291,0x239f,0x24b1,0x25c8,0x26e2,0x2801,
        0x2923,0x2a4a,0x2b74,0x2ca3,0x2dd6,0x2f0d,0x3047,0x3186,0x32c9,0x3411,0x355c,0x36ab,
        0x37fe,0x3956,0x3ab1,0x3c10,0x3d74,0x3edb,0x4047,0x41b7,0x432b,0x44a2,0x461e,0x479e,
        0x4922,0x4aaa,0x4c37,0x4dc7,0x4f5b,0x50f3,0x5290,0x5430,0x55d5,0x577d,0x592a,0x5adb,
        0x5c90,0x5e48,0x6005,0x61c6,0x638b,0x6554,0x6722,0x68f3,0x6ac8,0x6ca1,0x6e7f,0x7060,
        0x7246,0x7430,0x761d,0x780f,0x7a05,0x7bff,0x7dfd,0x7fff
    };

    public BinaryReader reader;
    public List<FileTableRecord> fileTableRecords = new List<FileTableRecord>();
    public List<DataFileHeader> dataFileHeaders = new List<DataFileHeader>();
    public uint key1;
    public uint key2;
    
    public static byte[] UIntArrayToByteArray(uint[] ints)
    {
        byte[] byteArray = new byte[ints.Length * 4];
        Buffer.BlockCopy(ints, 0, byteArray, 0, ints.Length * 4);
        return byteArray;
    }

    unsafe void DecryptOriginal(byte* buffer, uint byteCount, uint key1, uint key2)
    {
        //byte* pKey;
        uint byteCount64;
        long* buffer64;
        byte* pByte;
        uint cBytes;
        uint[] keys = { key2, key1 };

        // Process 64-bit chunks first, then process remaining bytes
        buffer64 = (long*)buffer;
        byteCount64 = byteCount / 8;

        for (; byteCount64 > 0; --byteCount64, ++buffer64)
        {
            uint ulongEx;
            uint* pLong = ((uint*)buffer64);

            ulongEx = *pLong;
            *pLong = ~((~ulongEx) ^ key2);
            ++pLong;
            ulongEx = *pLong;
            *pLong = ~((~ulongEx) ^ key1);
        }

        pByte = ((byte*)buffer64);
        cBytes = byteCount % 8;
        
        fixed (byte* pKey = UIntArrayToByteArray(keys)) {

            byte* currentKey = pKey;
            for (; cBytes > 0; --cBytes, ++pByte, ++currentKey) {
                byte currentbyte  = *pByte;
                byte key = *currentKey;
                *pByte = (byte)(~((~currentbyte) ^ key));
            }
        }
    }

    public unsafe void Decrypt(byte[] from, byte* to)
    {
        fixed (byte* bytesInput = from) {

            DecryptOriginal(bytesInput, (uint)from.Length, key1, key2);

            for (var i = 0; i < from.Length; i++)
                to[i] = bytesInput[i];
        }
    }

    public unsafe void Decrypt(byte* from, byte* to, int size)
    {
        DecryptOriginal(from, (uint)size, key1, key2);

        for (var i = 0; i < size; i++)
            to[i] = from[i];   
    }

    public unsafe void Decrypt(ref byte[] inout)
    {
        fixed (byte* bytesInput = inout)
        {
            DecryptOriginal(bytesInput, (uint)inout.Length, key1, key2);

            for (var i = 0; i < inout.Length; i++)
                inout[i] = bytesInput[i];
        }
    }

    public List<string> fileNames = new List<string>();

    public unsafe void Load(ref BinaryReader reader)
    {
        this.reader = reader;

        uint fileMagic = reader.ReadUInt32();

        if (fileMagic != 0x30445349)
        {
            Console.WriteLine("Bad file format");
            return;
        }
        
        //Read DTA header
        FileHeader newHeader = new FileHeader();
        Decrypt(reader.ReadBytes(sizeof(FileHeader)), (byte*)&newHeader);

        //Seek to files section
        reader.BaseStream.Seek(newHeader.fileTableOffset, SeekOrigin.Begin);

        //Clean previous file headers & read all headers at once
        fileTableRecords.Clear();

        byte[] allFileRecords = reader.ReadBytes(sizeof(FileTableRecord) * (int)newHeader.fileCount);
        Decrypt(ref allFileRecords);

        var offset = 0;

        for (var i = 0; i < (int)newHeader.fileCount; i++)
        {
            var newFileRecord = new FileTableRecord();
            byte* recordBytes = (byte*)&newFileRecord;
            
            for (var j = 0; j < sizeof(FileTableRecord); j++)
                recordBytes[j] = allFileRecords[offset + j];
            
            fileTableRecords.Add(newFileRecord);
            offset += sizeof(FileTableRecord);
        }

        //Go trough all file header seek, and get DTA file header
        for (var i = 0; i < fileTableRecords.Count; i++)
        {
            var currentHeader = fileTableRecords[i];
            reader.BaseStream.Seek(currentHeader.headerOffset, SeekOrigin.Begin);

            DataFileHeader newDataFileHeader = new DataFileHeader();
            Decrypt(reader.ReadBytes(sizeof(DataFileHeader)), (byte*)&newDataFileHeader);

            byte[] fileName = reader.ReadBytes(newDataFileHeader.nameLength);
            Decrypt(ref fileName);

            //save file name for later :)
            fileNames.Add(System.Text.Encoding.ASCII.GetString(fileName).ToLower());
            dataFileHeaders.Add(newDataFileHeader);
        }
    }

    public int GetFileIndex(string fileName)
    {
        int idxCounter = 0;
        foreach (var name in fileNames)
        {
            if (name == fileName) return idxCounter;

            idxCounter++;
        }

        return -1;
    }

    public bool wavHeaderRead = false;

    public unsafe void BufferCopyFrom(ref byte[] buffer, uint offset, byte* from, uint size)
    {
        fixed (byte* bufferPosPtr = buffer)
        {
            for (var x = 0; x < size; x++)
            {
                bufferPosPtr[offset + x] = from[x];
            }
        }
    }

    private unsafe List<byte> decompressLZSS(byte* buffer, uint bufferLen)
    {
        // rewritten version of hdmaster's source
        uint position = 0;
        List<byte> decompressed = new List<byte>();

        while (position < bufferLen)
        {
            int value = (buffer[position] << 8) | (buffer[position + 1]);  // get first two bytes => 16bit LZSS group
            position += 2;

            if (value == 0)  // 0 means just copy the next (at most) 16 bytes
            {
                uint n = Math.Min(bufferLen - position, 16);

                for (uint j = 0; j < n; ++j)
                    decompressed.Add(buffer[position + j]);

                position += n;
            }
            else
            {
                // go bit by bit in the group from the left
                for (uint i = 0; i < 16 && position < bufferLen; ++i, value <<= 1)
                {
                    if ((value & 0x8000) != 0)    // bit set => copy a sequence from already decompressed sequence začina 28
                    {
                        int offset = (buffer[(int)position] << 4) | (buffer[(int)position + 1] >> 4); //funguje 
                        uint n = (uint)buffer[(int)position + 1] & 0x0f; //funguje

                        // offset points back to already decompressed bytes, n defines the sequence length
                        if (offset == 0)
                        {
                            n = ((n << 8) | (buffer[position + 2])) + 16;
                           
                            for (var x = 0; x < n; x++)
                                decompressed.Add(buffer[position + 3]);

                            position += 4;
                        }
                        else
                        {
                            n += 3;
                            if (n > offset)
                            {
                                for (uint j = 0; j < n; ++j)
                                    decompressed.Add(decompressed[decompressed.Count - offset]);
                            }
                            else
                            {
                                var from = decompressed.Count - offset;
                                for (var j = from; j < from + n; j++)
                                    decompressed.Add(decompressed[j]);
                            }

                            position += 2;
                        }
                    }
                    else    
                    {
                        decompressed.Add(buffer[position]);
                        position++;
                    }
                }
            }
        }

        return decompressed;
    }

    private void BufferInsert(ref List<byte> list, int count, byte value)
    {
        for (var i = 0; i < count; i++)
            list.Add(value);
    }

    public unsafe byte[] GetFile(string fileName)
    {
        var fileIdx = GetFileIndex(fileName);
        if (fileIdx == -1)
        {
            Console.WriteLine("Unable to find file in dta !");
            return null;
        }

        var fileOffset = fileTableRecords[fileIdx].dataOffset;
        var fileHeader = dataFileHeaders[fileIdx];
        var fileSize = fileHeader.fileSize;

        //alloc new buffer result file
        byte[] dstBuffer = new byte[fileSize];
        reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);

        uint bufferPos = 0;

        //test for that flag
        bool blockEncrypted = (fileHeader.flags[0] & 0x80) == 0x80;

        for (var i = 0; i < fileHeader.compressedBlockCount; ++i)
        {
            // FIXME: make this function nicer // compressed size
            uint blockSize = reader.ReadUInt32() & 0xffff;
           
            byte[] block = reader.ReadBytes((int)blockSize);
            if (blockEncrypted)
                Decrypt(ref block);

            BlockType blockType = (BlockType)block[0];

            // 1 - don't include the type byte
            fixed (byte* blockToAdvance = block)  
                BufferCopyFrom(ref dstBuffer, bufferPos, blockToAdvance + 1, blockSize - 1);
            

            List<byte> decompressed = new List<byte>();

            int dpcmType = -1;

            switch (blockType)
            {
                case BlockType.Uncompressed:
                {
                    for (uint j = 0; j < blockSize - 1; ++j) // just copy the data
                        decompressed.Add(block[1 + j]);
                }
                break;

                case BlockType.LZSS_RLE:
                {
                    fixed(byte* dstBytes = dstBuffer)
                        decompressed = decompressLZSS(dstBytes + bufferPos, blockSize - 1);
                }
                break;

                case BlockType.DPCM0: dpcmType = 0; break;
                case BlockType.DPCM1: dpcmType = 1; break;
                case BlockType.DPCM2: dpcmType = 2; break;
                case BlockType.DPCM3: dpcmType = 3; break;
                case BlockType.DPCM4: dpcmType = 4; break;
                case BlockType.DPCM5: dpcmType = 5; break;
                case BlockType.DPCM6: dpcmType = 6; break;
                default: break;
            }

            if (dpcmType >= 0) 
                fixed(byte* dstBytes = dstBuffer)
                    fixed (ushort* deltas = WavDeltas)
                        decompressed = decompressDPCM(deltas + (128 * dpcmType), dstBytes + bufferPos, blockSize - 1);
            
            fixed (byte* decompressedBytes = decompressed.ToArray())
                BufferCopyFrom(ref dstBuffer, bufferPos, decompressedBytes, (uint)decompressed.Count);
            
            bufferPos += (uint)decompressed.Count;
        }

        return dstBuffer;
    }

    private unsafe List<byte> decompressDPCM(ushort* delta, byte* buffer, uint bufferLen)
    {
        int position = 0;
        List<byte> decompressed = new List<byte>();

        if (!wavHeaderRead)
        {
            position += sizeof(WavHeader);

            var newHeader = new WavHeader();
            byte* wavHeaderBytes = (byte*)&newHeader;

            Decrypt(buffer, wavHeaderBytes, sizeof(WavHeader));

            for(var i = 0; i < sizeof(WavHeader); i++)            
                decompressed.Add(wavHeaderBytes[i]);

            wavHeader = newHeader;
            wavHeaderRead = true;
        }

        if (wavHeader.channels == 1)
        {
            decompressed.Add(buffer[position]);
            decompressed.Add(buffer[position + 1]);

            ushort value = *((ushort*)&(buffer[position]));

            position += 2;

            while (position < bufferLen)
            {
                int sign = (buffer[position] & 0x80) == 0 ? 1 : -1;
                value += (ushort)(sign * delta[buffer[position] & 0x7f]);
                decompressed.Add((byte)(value & 0xff));
                decompressed.Add((byte)(value >> 8));
                position += 1;
            }
        }
        else
        {
            // TODO: find a stereo WAV and implement this or prove there are no stereo WAVs
        }

        return decompressed;
    }
}