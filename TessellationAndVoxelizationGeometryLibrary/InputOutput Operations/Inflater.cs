﻿// Decompiled with JetBrains decompiler
// Type: System.IO.Compression.Inflater
// Assembly: System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 67F338CA-9799-462C-9779-9C54DB00C2DD
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.IO.Compression
{
    internal class Inflater
    {
        private static readonly byte[] extraLengthBits = new byte[29]
        {
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 1,
      (byte) 1,
      (byte) 1,
      (byte) 1,
      (byte) 2,
      (byte) 2,
      (byte) 2,
      (byte) 2,
      (byte) 3,
      (byte) 3,
      (byte) 3,
      (byte) 3,
      (byte) 4,
      (byte) 4,
      (byte) 4,
      (byte) 4,
      (byte) 5,
      (byte) 5,
      (byte) 5,
      (byte) 5,
      (byte) 0
        };
        private static readonly int[] lengthBase = new int[29]
        {
      3,
      4,
      5,
      6,
      7,
      8,
      9,
      10,
      11,
      13,
      15,
      17,
      19,
      23,
      27,
      31,
      35,
      43,
      51,
      59,
      67,
      83,
      99,
      115,
      131,
      163,
      195,
      227,
      258
        };
        private static readonly int[] distanceBasePosition = new int[32]
        {
      1,
      2,
      3,
      4,
      5,
      7,
      9,
      13,
      17,
      25,
      33,
      49,
      65,
      97,
      129,
      193,
      257,
      385,
      513,
      769,
      1025,
      1537,
      2049,
      3073,
      4097,
      6145,
      8193,
      12289,
      16385,
      24577,
      0,
      0
        };
        private static readonly byte[] codeOrder = new byte[19]
        {
      (byte) 16,
      (byte) 17,
      (byte) 18,
      (byte) 0,
      (byte) 8,
      (byte) 7,
      (byte) 9,
      (byte) 6,
      (byte) 10,
      (byte) 5,
      (byte) 11,
      (byte) 4,
      (byte) 12,
      (byte) 3,
      (byte) 13,
      (byte) 2,
      (byte) 14,
      (byte) 1,
      (byte) 15
        };
        private static readonly byte[] staticDistanceTreeTable = new byte[32]
        {
      (byte) 0,
      (byte) 16,
      (byte) 8,
      (byte) 24,
      (byte) 4,
      (byte) 20,
      (byte) 12,
      (byte) 28,
      (byte) 2,
      (byte) 18,
      (byte) 10,
      (byte) 26,
      (byte) 6,
      (byte) 22,
      (byte) 14,
      (byte) 30,
      (byte) 1,
      (byte) 17,
      (byte) 9,
      (byte) 25,
      (byte) 5,
      (byte) 21,
      (byte) 13,
      (byte) 29,
      (byte) 3,
      (byte) 19,
      (byte) 11,
      (byte) 27,
      (byte) 7,
      (byte) 23,
      (byte) 15,
      (byte) 31
        };
        private byte[] blockLengthBuffer = new byte[4];
        private OutputWindow output;
        private InputBuffer input;
        private HuffmanTree literalLengthTree;
        private HuffmanTree distanceTree;
        private InflaterState state;
        private bool hasFormatReader;
        private int bfinal;
        private BlockType blockType;
        private int blockLength;
        private int length;
        private int distanceCode;
        private int extraBits;
        private int loopCounter;
        private int literalLengthCodeCount;
        private int distanceCodeCount;
        private int codeLengthCodeCount;
        private int codeArraySize;
        private int lengthCode;
        private byte[] codeList;
        private byte[] codeLengthTreeCodeLength;
        private HuffmanTree codeLengthTree;
        private IFileFormatReader formatReader;

        public int AvailableOutput
        {
            get
            {
                return this.output.AvailableBytes;
            }
        }

        public Inflater()
        {
            this.output = new OutputWindow();
            this.input = new InputBuffer();
            this.codeList = new byte[320];
            this.codeLengthTreeCodeLength = new byte[19];
            this.Reset();
        }

        internal void SetFileFormatReader(IFileFormatReader reader)
        {
            this.formatReader = reader;
            this.hasFormatReader = true;
            this.Reset();
        }

        private void Reset()
        {
            if (this.hasFormatReader)
                this.state = InflaterState.ReadingHeader;
            else
                this.state = InflaterState.ReadingBFinal;
        }

        public void SetInput(byte[] inputBytes, int offset, int length)
        {
            this.input.SetInput(inputBytes, offset, length);
        }

        public bool Finished()
        {
            if (this.state != InflaterState.Done)
                return this.state == InflaterState.VerifyingFooter;
            return true;
        }

        public bool NeedsInput()
        {
            return this.input.NeedsInput();
        }

        public int Inflate(byte[] bytes, int offset, int length)
        {
            int num = 0;
            do
            {
                int bytesToCopy = this.output.CopyTo(bytes, offset, length);
                if (bytesToCopy > 0)
                {
                    if (this.hasFormatReader)
                        this.formatReader.UpdateWithBytesRead(bytes, offset, bytesToCopy);
                    offset += bytesToCopy;
                    num += bytesToCopy;
                    length -= bytesToCopy;
                }
            }
            while (length != 0 && !this.Finished() && this.Decode());
            if (this.state == InflaterState.VerifyingFooter && this.output.AvailableBytes == 0)
                this.formatReader.Validate();
            return num;
        }

        private bool Decode()
        {
            bool flag1 = false;
            if (this.Finished())
                return true;
            if (this.hasFormatReader)
            {
                if (this.state == InflaterState.ReadingHeader)
                {
                    if (!this.formatReader.ReadHeader(this.input))
                        return false;
                    this.state = InflaterState.ReadingBFinal;
                }
                else if (this.state == InflaterState.StartReadingFooter || this.state == InflaterState.ReadingFooter)
                {
                    if (!this.formatReader.ReadFooter(this.input))
                        return false;
                    this.state = InflaterState.VerifyingFooter;
                    return true;
                }
            }
            if (this.state == InflaterState.ReadingBFinal)
            {
                if (!this.input.EnsureBitsAvailable(1))
                    return false;
                this.bfinal = this.input.GetBits(1);
                this.state = InflaterState.ReadingBType;
            }
            if (this.state == InflaterState.ReadingBType)
            {
                if (!this.input.EnsureBitsAvailable(2))
                {
                    this.state = InflaterState.ReadingBType;
                    return false;
                }
                this.blockType = (BlockType)this.input.GetBits(2);
                if (this.blockType == BlockType.Dynamic)
                    this.state = InflaterState.ReadingNumLitCodes;
                else if (this.blockType == BlockType.Static)
                {
                    this.literalLengthTree = HuffmanTree.StaticLiteralLengthTree;
                    this.distanceTree = HuffmanTree.StaticDistanceTree;
                    this.state = InflaterState.DecodeTop;
                }
                else
                {
                    if (this.blockType != BlockType.Uncompressed)
                        throw new InvalidDataException(SR.GetString("UnknownBlockType"));
                    this.state = InflaterState.UncompressedAligning;
                }
            }
            bool flag2;
            if (this.blockType == BlockType.Dynamic)
                flag2 = this.state >= InflaterState.DecodeTop ? this.DecodeBlock(out flag1) : this.DecodeDynamicBlockHeader();
            else if (this.blockType == BlockType.Static)
            {
                flag2 = this.DecodeBlock(out flag1);
            }
            else
            {
                if (this.blockType != BlockType.Uncompressed)
                    throw new InvalidDataException(SR.GetString("UnknownBlockType"));
                flag2 = this.DecodeUncompressedBlock(out flag1);
            }
            if (flag1 && this.bfinal != 0)
                this.state = !this.hasFormatReader ? InflaterState.Done : InflaterState.StartReadingFooter;
            return flag2;
        }

        private bool DecodeUncompressedBlock(out bool end_of_block)
        {
            end_of_block = false;
            while (true)
            {
                switch (this.state)
                {
                    case InflaterState.UncompressedAligning:
                        this.input.SkipToByteBoundary();
                        this.state = InflaterState.UncompressedByte1;
                        goto case InflaterState.UncompressedByte1;
                    case InflaterState.UncompressedByte1:
                    case InflaterState.UncompressedByte2:
                    case InflaterState.UncompressedByte3:
                    case InflaterState.UncompressedByte4:
                        int bits = this.input.GetBits(8);
                        if (bits >= 0)
                        {
                            this.blockLengthBuffer[(int)(this.state - 16)] = (byte)bits;
                            if (this.state == InflaterState.UncompressedByte4)
                            {
                                this.blockLength = (int)this.blockLengthBuffer[0] + (int)this.blockLengthBuffer[1] * 256;
                                if ((int)(ushort)this.blockLength != (int)(ushort)~((int)this.blockLengthBuffer[2] + (int)this.blockLengthBuffer[3] * 256))
                                    goto label_7;
                            }
                            this.state = this.state + 1;
                            continue;
                        }
                        goto label_4;
                    case InflaterState.DecodingUncompressed:
                        goto label_9;
                    default:
                        goto label_14;
                }
            }
            label_4:
            return false;
            label_7:
            throw new InvalidDataException(SR.GetString("InvalidBlockLength"));
            label_9:
            this.blockLength = this.blockLength - this.output.CopyFrom(this.input, this.blockLength);
            if (this.blockLength == 0)
            {
                this.state = InflaterState.ReadingBFinal;
                end_of_block = true;
                return true;
            }
            return this.output.FreeBytes == 0;
            label_14:
            throw new InvalidDataException(SR.GetString("UnknownState"));
        }

        private bool DecodeBlock(out bool end_of_block_code_seen)
        {
            end_of_block_code_seen = false;
            int freeBytes = this.output.FreeBytes;
            while (freeBytes > 258)
            {
                switch (this.state)
                {
                    case InflaterState.DecodeTop:
                        int nextSymbol = this.literalLengthTree.GetNextSymbol(this.input);
                        if (nextSymbol < 0)
                            return false;
                        if (nextSymbol < 256)
                        {
                            this.output.Write((byte)nextSymbol);
                            --freeBytes;
                            continue;
                        }
                        if (nextSymbol == 256)
                        {
                            end_of_block_code_seen = true;
                            this.state = InflaterState.ReadingBFinal;
                            return true;
                        }
                        int index = nextSymbol - 257;
                        if (index < 8)
                        {
                            index += 3;
                            this.extraBits = 0;
                        }
                        else if (index == 28)
                        {
                            index = 258;
                            this.extraBits = 0;
                        }
                        else
                        {
                            if (index < 0 || index >= Inflater.extraLengthBits.Length)
                                throw new InvalidDataException(SR.GetString("GenericInvalidData"));
                            this.extraBits = (int)Inflater.extraLengthBits[index];
                        }
                        this.length = index;
                        goto case InflaterState.HaveInitialLength;
                    case InflaterState.HaveInitialLength:
                        if (this.extraBits > 0)
                        {
                            this.state = InflaterState.HaveInitialLength;
                            int bits = this.input.GetBits(this.extraBits);
                            if (bits < 0)
                                return false;
                            if (this.length < 0 || this.length >= Inflater.lengthBase.Length)
                                throw new InvalidDataException(SR.GetString("GenericInvalidData"));
                            this.length = Inflater.lengthBase[this.length] + bits;
                        }
                        this.state = InflaterState.HaveFullLength;
                        goto case InflaterState.HaveFullLength;
                    case InflaterState.HaveFullLength:
                        if (this.blockType == BlockType.Dynamic)
                        {
                            this.distanceCode = this.distanceTree.GetNextSymbol(this.input);
                        }
                        else
                        {
                            this.distanceCode = this.input.GetBits(5);
                            if (this.distanceCode >= 0)
                                this.distanceCode = (int)Inflater.staticDistanceTreeTable[this.distanceCode];
                        }
                        if (this.distanceCode < 0)
                            return false;
                        this.state = InflaterState.HaveDistCode;
                        goto case InflaterState.HaveDistCode;
                    case InflaterState.HaveDistCode:
                        int distance;
                        if (this.distanceCode > 3)
                        {
                            this.extraBits = this.distanceCode - 2 >> 1;
                            int bits = this.input.GetBits(this.extraBits);
                            if (bits < 0)
                                return false;
                            distance = Inflater.distanceBasePosition[this.distanceCode] + bits;
                        }
                        else
                            distance = this.distanceCode + 1;
                        this.output.WriteLengthDistance(this.length, distance);
                        freeBytes -= this.length;
                        this.state = InflaterState.DecodeTop;
                        continue;
                    default:
                        throw new InvalidDataException(SR.GetString("UnknownState"));
                }
            }
            return true;
        }

        private bool DecodeDynamicBlockHeader()
        {
            switch (this.state)
            {
                case InflaterState.ReadingNumLitCodes:
                    this.literalLengthCodeCount = this.input.GetBits(5);
                    if (this.literalLengthCodeCount < 0)
                        return false;
                    this.literalLengthCodeCount = this.literalLengthCodeCount + 257;
                    this.state = InflaterState.ReadingNumDistCodes;
                    goto case InflaterState.ReadingNumDistCodes;
                case InflaterState.ReadingNumDistCodes:
                    this.distanceCodeCount = this.input.GetBits(5);
                    if (this.distanceCodeCount < 0)
                        return false;
                    this.distanceCodeCount = this.distanceCodeCount + 1;
                    this.state = InflaterState.ReadingNumCodeLengthCodes;
                    goto case InflaterState.ReadingNumCodeLengthCodes;
                case InflaterState.ReadingNumCodeLengthCodes:
                    this.codeLengthCodeCount = this.input.GetBits(4);
                    if (this.codeLengthCodeCount < 0)
                        return false;
                    this.codeLengthCodeCount = this.codeLengthCodeCount + 4;
                    this.loopCounter = 0;
                    this.state = InflaterState.ReadingCodeLengthCodes;
                    goto case InflaterState.ReadingCodeLengthCodes;
                case InflaterState.ReadingCodeLengthCodes:
                    for (; this.loopCounter < this.codeLengthCodeCount; this.loopCounter = this.loopCounter + 1)
                    {
                        int bits = this.input.GetBits(3);
                        if (bits < 0)
                            return false;
                        this.codeLengthTreeCodeLength[(int)Inflater.codeOrder[this.loopCounter]] = (byte)bits;
                    }
                    for (int index = this.codeLengthCodeCount; index < Inflater.codeOrder.Length; ++index)
                        this.codeLengthTreeCodeLength[(int)Inflater.codeOrder[index]] = (byte)0;
                    this.codeLengthTree = new HuffmanTree(this.codeLengthTreeCodeLength);
                    this.codeArraySize = this.literalLengthCodeCount + this.distanceCodeCount;
                    this.loopCounter = 0;
                    this.state = InflaterState.ReadingTreeCodesBefore;
                    goto case InflaterState.ReadingTreeCodesBefore;
                case InflaterState.ReadingTreeCodesBefore:
                case InflaterState.ReadingTreeCodesAfter:
                    while (this.loopCounter < this.codeArraySize)
                    {
                        if (this.state == InflaterState.ReadingTreeCodesBefore && (this.lengthCode = this.codeLengthTree.GetNextSymbol(this.input)) < 0)
                            return false;
                        if (this.lengthCode <= 15)
                        {
                            byte[] numArray = this.codeList;
                            int num1 = this.loopCounter;
                            this.loopCounter = num1 + 1;
                            int index = num1;
                            int num2 = (int)(byte)this.lengthCode;
                            numArray[index] = (byte)num2;
                        }
                        else
                        {
                            if (!this.input.EnsureBitsAvailable(7))
                            {
                                this.state = InflaterState.ReadingTreeCodesAfter;
                                return false;
                            }
                            if (this.lengthCode == 16)
                            {
                                if (this.loopCounter == 0)
                                    throw new InvalidDataException();
                                byte num1 = this.codeList[this.loopCounter - 1];
                                int num2 = this.input.GetBits(2) + 3;
                                if (this.loopCounter + num2 > this.codeArraySize)
                                    throw new InvalidDataException();
                                for (int index1 = 0; index1 < num2; ++index1)
                                {
                                    byte[] numArray = this.codeList;
                                    int num3 = this.loopCounter;
                                    this.loopCounter = num3 + 1;
                                    int index2 = num3;
                                    int num4 = (int)num1;
                                    numArray[index2] = (byte)num4;
                                }
                            }
                            else if (this.lengthCode == 17)
                            {
                                int num1 = this.input.GetBits(3) + 3;
                                if (this.loopCounter + num1 > this.codeArraySize)
                                    throw new InvalidDataException();
                                for (int index1 = 0; index1 < num1; ++index1)
                                {
                                    byte[] numArray = this.codeList;
                                    int num2 = this.loopCounter;
                                    this.loopCounter = num2 + 1;
                                    int index2 = num2;
                                    int num3 = 0;
                                    numArray[index2] = (byte)num3;
                                }
                            }
                            else
                            {
                                int num1 = this.input.GetBits(7) + 11;
                                if (this.loopCounter + num1 > this.codeArraySize)
                                    throw new InvalidDataException();
                                for (int index1 = 0; index1 < num1; ++index1)
                                {
                                    byte[] numArray = this.codeList;
                                    int num2 = this.loopCounter;
                                    this.loopCounter = num2 + 1;
                                    int index2 = num2;
                                    int num3 = 0;
                                    numArray[index2] = (byte)num3;
                                }
                            }
                        }
                        this.state = InflaterState.ReadingTreeCodesBefore;
                    }
                    byte[] codeLengths1 = new byte[288];
                    byte[] codeLengths2 = new byte[32];
                    Array.Copy((Array)this.codeList, (Array)codeLengths1, this.literalLengthCodeCount);
                    Array.Copy((Array)this.codeList, this.literalLengthCodeCount, (Array)codeLengths2, 0, this.distanceCodeCount);
                    if ((int)codeLengths1[256] == 0)
                        throw new InvalidDataException();
                    this.literalLengthTree = new HuffmanTree(codeLengths1);
                    this.distanceTree = new HuffmanTree(codeLengths2);
                    this.state = InflaterState.DecodeTop;
                    return true;
                default:
                    throw new InvalidDataException(SR.GetString("UnknownState"));
            }
        }
    }
    /// <summary>
    /// Provides methods and properties for compressing and decompressing streams by using the Deflate algorithm.
    /// </summary>
    [__DynamicallyInvokable]
    public class DeflateStream : Stream
    {
        private static volatile DeflateStream.WorkerType deflaterType = DeflateStream.WorkerType.Unknown;
        internal const int DefaultBufferSize = 8192;
        private Stream _stream;
        private CompressionMode _mode;
        private bool _leaveOpen;
        private Inflater inflater;
        private IDeflater deflater;
        private byte[] buffer;
        private int asyncOperations;
        private readonly AsyncCallback m_CallBack;
        private readonly DeflateStream.AsyncWriteDelegate m_AsyncWriterDelegate;
        private IFileFormatWriter formatWriter;
        private bool wroteHeader;
        private bool wroteBytes;

        /// <summary>
        /// Gets a reference to the underlying stream.
        /// </summary>
        /// 
        /// <returns>
        /// A stream object that represents the underlying stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The underlying stream is closed.</exception>
        [__DynamicallyInvokable]
        public Stream BaseStream
        {
            [__DynamicallyInvokable]
            get
            {
                return this._stream;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports reading while decompressing a file.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.IO.Compression.CompressionMode"/> value is Decompress, and the underlying stream is opened and supports reading; otherwise, false.
        /// </returns>
        [__DynamicallyInvokable]
        public override bool CanRead
        {
            [__DynamicallyInvokable]
            get
            {
                if (this._stream == null || this._mode != CompressionMode.Decompress)
                    return false;
                return this._stream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports writing.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.IO.Compression.CompressionMode"/> value is Compress, and the underlying stream supports writing and is not closed; otherwise, false.
        /// </returns>
        [__DynamicallyInvokable]
        public override bool CanWrite
        {
            [__DynamicallyInvokable]
            get
            {
                if (this._stream == null || this._mode != CompressionMode.Compress)
                    return false;
                return this._stream.CanWrite;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking.
        /// </summary>
        /// 
        /// <returns>
        /// false in all cases.
        /// </returns>
        [__DynamicallyInvokable]
        public override bool CanSeek
        {
            [__DynamicallyInvokable]
            get
            {
                return false;
            }
        }

        /// <summary>
        /// This property is not supported and always throws a <see cref="T:System.NotSupportedException"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A long value.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        [__DynamicallyInvokable]
        public override long Length
        {
            [__DynamicallyInvokable]
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
        }

        /// <summary>
        /// This property is not supported and always throws a <see cref="T:System.NotSupportedException"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A long value.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        [__DynamicallyInvokable]
        public override long Position
        {
            [__DynamicallyInvokable]
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
            [__DynamicallyInvokable]
            set
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.Compression.DeflateStream"/> class by using the specified stream and compression mode.
        /// </summary>
        /// <param name="stream">The stream to compress or decompress.</param><param name="mode">One of the enumeration values that indicates whether to compress or decompress the stream.</param><exception cref="T:System.ArgumentNullException"><paramref name="stream"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="mode"/> is not a valid <see cref="T:System.IO.Compression.CompressionMode"/> value.-or-<see cref="T:System.IO.Compression.CompressionMode"/> is <see cref="F:System.IO.Compression.CompressionMode.Compress"/>  and <see cref="P:System.IO.Stream.CanWrite"/> is false.-or-<see cref="T:System.IO.Compression.CompressionMode"/> is <see cref="F:System.IO.Compression.CompressionMode.Decompress"/>  and <see cref="P:System.IO.Stream.CanRead"/> is false.</exception>
        [__DynamicallyInvokable]
        public DeflateStream(Stream stream, CompressionMode mode)
          : this(stream, mode, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.Compression.DeflateStream"/> class by using the specified stream and compression mode, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The stream to compress or decompress.</param><param name="mode">One of the enumeration values that indicates whether to compress or decompress the stream.</param><param name="leaveOpen">true to leave the stream open after disposing the <see cref="T:System.IO.Compression.DeflateStream"/> object; otherwise, false.</param><exception cref="T:System.ArgumentNullException"><paramref name="stream"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="mode"/> is not a valid <see cref="T:System.IO.Compression.CompressionMode"/> value.-or-<see cref="T:System.IO.Compression.CompressionMode"/> is <see cref="F:System.IO.Compression.CompressionMode.Compress"/>  and <see cref="P:System.IO.Stream.CanWrite"/> is false.-or-<see cref="T:System.IO.Compression.CompressionMode"/> is <see cref="F:System.IO.Compression.CompressionMode.Decompress"/>  and <see cref="P:System.IO.Stream.CanRead"/> is false.</exception>
        [__DynamicallyInvokable]
        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (CompressionMode.Compress != mode && mode != CompressionMode.Decompress)
                throw new ArgumentException(SR.GetString("ArgumentOutOfRange_Enum"), "mode");
            this._stream = stream;
            this._mode = mode;
            this._leaveOpen = leaveOpen;
            switch (this._mode)
            {
                case CompressionMode.Decompress:
                    if (!this._stream.CanRead)
                        throw new ArgumentException(SR.GetString("NotReadableStream"), "stream");
                    this.inflater = new Inflater();
                    this.m_CallBack = new AsyncCallback(this.ReadCallback);
                    break;
                case CompressionMode.Compress:
                    if (!this._stream.CanWrite)
                        throw new ArgumentException(SR.GetString("NotWriteableStream"), "stream");
                    this.deflater = DeflateStream.CreateDeflater(new CompressionLevel?());
                    this.m_AsyncWriterDelegate = new DeflateStream.AsyncWriteDelegate(this.InternalWrite);
                    this.m_CallBack = new AsyncCallback(this.WriteCallback);
                    break;
            }
            this.buffer = new byte[8192];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.Compression.DeflateStream"/> class by using the specified stream and compression level.
        /// </summary>
        /// <param name="stream">The stream to compress.</param><param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression efficiency when compressing the stream.</param><exception cref="T:System.ArgumentNullException"><paramref name="stream"/> is null.</exception><exception cref="T:System.ArgumentException">The stream does not support write operations such as compression. (The <see cref="P:System.IO.Stream.CanWrite"/> property on the stream object is false.)</exception>
        [__DynamicallyInvokable]
        public DeflateStream(Stream stream, CompressionLevel compressionLevel)
          : this(stream, compressionLevel, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.Compression.DeflateStream"/> class by using the specified stream and compression level, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The stream to compress.</param><param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression efficiency when compressing the stream.</param><param name="leaveOpen">true to leave the stream object open after disposing the <see cref="T:System.IO.Compression.DeflateStream"/> object; otherwise, false.</param><exception cref="T:System.ArgumentNullException"><paramref name="stream"/> is null.</exception><exception cref="T:System.ArgumentException">The stream does not support write operations such as compression. (The <see cref="P:System.IO.Stream.CanWrite"/> property on the stream object is false.)</exception>
        [__DynamicallyInvokable]
        public DeflateStream(Stream stream, CompressionLevel compressionLevel, bool leaveOpen)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (!stream.CanWrite)
                throw new ArgumentException(SR.GetString("NotWriteableStream"), "stream");
            this._stream = stream;
            this._mode = CompressionMode.Compress;
            this._leaveOpen = leaveOpen;
            this.deflater = DeflateStream.CreateDeflater(new CompressionLevel?(compressionLevel));
            this.m_AsyncWriterDelegate = new DeflateStream.AsyncWriteDelegate(this.InternalWrite);
            this.m_CallBack = new AsyncCallback(this.WriteCallback);
            this.buffer = new byte[8192];
        }

        private static IDeflater CreateDeflater(CompressionLevel? compressionLevel)
        {
            switch (DeflateStream.GetDeflaterType())
            {
                case DeflateStream.WorkerType.Managed:
                    return (IDeflater)new DeflaterManaged();
                case DeflateStream.WorkerType.ZLib:
                    if (compressionLevel.HasValue)
                        return (IDeflater)new DeflaterZLib(compressionLevel.Value);
                    return (IDeflater)new DeflaterZLib();
                default:
                    throw new SystemException("Program entered an unexpected state.");
            }
        }

        [SecuritySafeCritical]
        private static DeflateStream.WorkerType GetDeflaterType()
        {
            if (DeflateStream.WorkerType.Unknown != DeflateStream.deflaterType)
                return DeflateStream.deflaterType;
            if (CLRConfig.CheckLegacyManagedDeflateStream() || CompatibilitySwitches.IsNetFx45LegacyManagedDeflateStream)
                return DeflateStream.deflaterType = DeflateStream.WorkerType.Managed;
            return DeflateStream.deflaterType = DeflateStream.WorkerType.ZLib;
        }

        internal void SetFileFormatReader(IFileFormatReader reader)
        {
            if (reader == null)
                return;
            this.inflater.SetFileFormatReader(reader);
        }

        internal void SetFileFormatWriter(IFileFormatWriter writer)
        {
            if (writer == null)
                return;
            this.formatWriter = writer;
        }

        /// <summary>
        /// The current implementation of this method has no functionality.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        [__DynamicallyInvokable]
        public override void Flush()
        {
            this.EnsureNotDisposed();
        }

        /// <summary>
        /// This operation is not supported and always throws a <see cref="T:System.NotSupportedException"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A long value.
        /// </returns>
        /// <param name="offset">The location in the stream.</param><param name="origin">One of the <see cref="T:System.IO.SeekOrigin"/> values.</param><exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
        [__DynamicallyInvokable]
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("NotSupported"));
        }

        /// <summary>
        /// This operation is not supported and always throws a <see cref="T:System.NotSupportedException"/>.
        /// </summary>
        /// <param name="value">The length of the stream.</param><exception cref="T:System.NotSupportedException">This property is not supported on this stream.</exception>
        [__DynamicallyInvokable]
        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("NotSupported"));
        }

        /// <summary>
        /// Reads a number of decompressed bytes into the specified byte array.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes that were read into the byte array.
        /// </returns>
        /// <param name="array">The array to store decompressed bytes.</param><param name="offset">The byte offset in <paramref name="array"/> at which the read bytes will be placed.</param><param name="count">The maximum number of decompressed bytes to read.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.InvalidOperationException">The <see cref="T:System.IO.Compression.CompressionMode"/> value was Compress when the object was created.- or - The underlying stream does not support reading.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is less than zero.-or-<paramref name="array"/> length minus the index starting point is less than <paramref name="count"/>.</exception><exception cref="T:System.IO.InvalidDataException">The data is in an invalid format.</exception><exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        [__DynamicallyInvokable]
        public override int Read(byte[] array, int offset, int count)
        {
            this.EnsureDecompressionMode();
            this.ValidateParameters(array, offset, count);
            this.EnsureNotDisposed();
            int offset1 = offset;
            int length1 = count;
            while (true)
            {
                int num = this.inflater.Inflate(array, offset1, length1);
                offset1 += num;
                length1 -= num;
                if (length1 != 0 && !this.inflater.Finished())
                {
                    int length2 = this._stream.Read(this.buffer, 0, this.buffer.Length);
                    if (length2 != 0)
                        this.inflater.SetInput(this.buffer, 0, length2);
                    else
                        break;
                }
                else
                    break;
            }
            return count - length1;
        }

        private void ValidateParameters(byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (array.Length - offset < count)
                throw new ArgumentException(SR.GetString("InvalidArgumentOffsetCount"));
        }

        private void EnsureNotDisposed()
        {
            if (this._stream == null)
                throw new ObjectDisposedException((string)null, SR.GetString("ObjectDisposed_StreamClosed"));
        }

        private void EnsureDecompressionMode()
        {
            if (this._mode != CompressionMode.Decompress)
                throw new InvalidOperationException(SR.GetString("CannotReadFromDeflateStream"));
        }

        private void EnsureCompressionMode()
        {
            if (this._mode != CompressionMode.Compress)
                throw new InvalidOperationException(SR.GetString("CannotWriteToDeflateStream"));
        }

        /// <summary>
        /// Begins an asynchronous read operation. (Consider using the <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)"/> method instead; see the Remarks section.)
        /// </summary>
        /// 
        /// <returns>
        /// An  object that represents the asynchronous read operation, which could still be pending.
        /// </returns>
        /// <param name="array">The byte array to read the data into.</param><param name="offset">The byte offset in <paramref name="array"/> at which to begin reading data from the stream.</param><param name="count">The maximum number of bytes to read.</param><param name="asyncCallback">An optional asynchronous callback, to be called when the read operation is complete.</param><param name="asyncState">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param><exception cref="T:System.IO.IOException">The method tried to read asynchronously past the end of the stream, or a disk error occurred.</exception><exception cref="T:System.ArgumentException">One or more of the arguments is invalid.</exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception><exception cref="T:System.NotSupportedException">The current <see cref="T:System.IO.Compression.DeflateStream"/> implementation does not support the read operation.</exception><exception cref="T:System.InvalidOperationException">This call cannot be completed. </exception>
        [__DynamicallyInvokable]
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            this.EnsureDecompressionMode();
            if (this.asyncOperations != 0)
                throw new InvalidOperationException(SR.GetString("InvalidBeginCall"));
            this.ValidateParameters(array, offset, count);
            this.EnsureNotDisposed();
            Interlocked.Increment(ref this.asyncOperations);
            try
            {
                DeflateStreamAsyncResult streamAsyncResult = new DeflateStreamAsyncResult((object)this, asyncState, asyncCallback, array, offset, count);
                streamAsyncResult.isWrite = false;
                int num = this.inflater.Inflate(array, offset, count);
                if (num != 0)
                {
                    streamAsyncResult.InvokeCallback(true, (object)num);
                    return (IAsyncResult)streamAsyncResult;
                }
                if (this.inflater.Finished())
                {
                    streamAsyncResult.InvokeCallback(true, (object)0);
                    return (IAsyncResult)streamAsyncResult;
                }
                this._stream.BeginRead(this.buffer, 0, this.buffer.Length, this.m_CallBack, (object)streamAsyncResult);
                streamAsyncResult.m_CompletedSynchronously &= streamAsyncResult.IsCompleted;
                return (IAsyncResult)streamAsyncResult;
            }
            catch
            {
                Interlocked.Decrement(ref this.asyncOperations);
                throw;
            }
        }

        private void ReadCallback(IAsyncResult baseStreamResult)
        {
            DeflateStreamAsyncResult streamAsyncResult = (DeflateStreamAsyncResult)baseStreamResult.AsyncState;
            streamAsyncResult.m_CompletedSynchronously &= baseStreamResult.CompletedSynchronously;
            try
            {
                this.EnsureNotDisposed();
                int length = this._stream.EndRead(baseStreamResult);
                if (length <= 0)
                {
                    streamAsyncResult.InvokeCallback((object)0);
                }
                else
                {
                    this.inflater.SetInput(this.buffer, 0, length);
                    int num = this.inflater.Inflate(streamAsyncResult.buffer, streamAsyncResult.offset, streamAsyncResult.count);
                    if (num == 0 && !this.inflater.Finished())
                        this._stream.BeginRead(this.buffer, 0, this.buffer.Length, this.m_CallBack, (object)streamAsyncResult);
                    else
                        streamAsyncResult.InvokeCallback((object)num);
                }
            }
            catch (Exception ex)
            {
                streamAsyncResult.InvokeCallback((object)ex);
            }
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete. (Consider using the <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)"/> method instead; see the Remarks section.)
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes read from the stream, between 0 (zero) and the number of bytes you requested. <see cref="T:System.IO.Compression.DeflateStream"/> returns 0 only at the end of the stream; otherwise, it blocks until at least one byte is available.
        /// </returns>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Compression.DeflateStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream.</exception><exception cref="T:System.SystemException">An exception was thrown during a call to <see cref="M:System.Threading.WaitHandle.WaitOne"/>.</exception><exception cref="T:System.InvalidOperationException">The end call is invalid because asynchronous read operations for this stream are not yet complete.</exception><exception cref="T:System.InvalidOperationException">The stream is null.</exception>
        [__DynamicallyInvokable]
        public override int EndRead(IAsyncResult asyncResult)
        {
            this.EnsureDecompressionMode();
            this.CheckEndXxxxLegalStateAndParams(asyncResult);
            DeflateStreamAsyncResult asyncResult1 = (DeflateStreamAsyncResult)asyncResult;
            this.AwaitAsyncResultCompletion(asyncResult1);
            Exception exception = asyncResult1.Result as Exception;
            if (exception != null)
                throw exception;
            return (int)asyncResult1.Result;
        }

        /// <summary>
        /// Writes compressed bytes to the underlying stream from the specified byte array.
        /// </summary>
        /// <param name="array">The buffer that contains the data to compress.</param><param name="offset">The byte offset in <paramref name="array"/> from which the bytes will be read.</param><param name="count">The maximum number of bytes to write.</param>
        [__DynamicallyInvokable]
        public override void Write(byte[] array, int offset, int count)
        {
            this.EnsureCompressionMode();
            this.ValidateParameters(array, offset, count);
            this.EnsureNotDisposed();
            this.InternalWrite(array, offset, count, false);
        }

        internal void InternalWrite(byte[] array, int offset, int count, bool isAsync)
        {
            this.DoMaintenance(array, offset, count);
            this.WriteDeflaterOutput(isAsync);
            this.deflater.SetInput(array, offset, count);
            this.WriteDeflaterOutput(isAsync);
        }

        private void WriteDeflaterOutput(bool isAsync)
        {
            while (!this.deflater.NeedsInput())
            {
                int deflateOutput = this.deflater.GetDeflateOutput(this.buffer);
                if (deflateOutput > 0)
                    this.DoWrite(this.buffer, 0, deflateOutput, isAsync);
            }
        }

        private void DoWrite(byte[] array, int offset, int count, bool isAsync)
        {
            if (isAsync)
                this._stream.EndWrite(this._stream.BeginWrite(array, offset, count, (AsyncCallback)null, (object)null));
            else
                this._stream.Write(array, offset, count);
        }

        private void DoMaintenance(byte[] array, int offset, int count)
        {
            if (count <= 0)
                return;
            this.wroteBytes = true;
            if (this.formatWriter == null)
                return;
            if (!this.wroteHeader)
            {
                byte[] header = this.formatWriter.GetHeader();
                this._stream.Write(header, 0, header.Length);
                this.wroteHeader = true;
            }
            this.formatWriter.UpdateWithBytesRead(array, offset, count);
        }

        private void PurgeBuffers(bool disposing)
        {
            if (!disposing || this._stream == null)
                return;
            this.Flush();
            if (this._mode != CompressionMode.Compress)
                return;
            if (this.wroteBytes)
            {
                this.WriteDeflaterOutput(false);
                bool flag;
                do
                {
                    int bytesRead;
                    flag = this.deflater.Finish(this.buffer, out bytesRead);
                    if (bytesRead > 0)
                        this.DoWrite(this.buffer, 0, bytesRead, false);
                }
                while (!flag);
            }
            if (this.formatWriter == null || !this.wroteHeader)
                return;
            byte[] footer = this.formatWriter.GetFooter();
            this._stream.Write(footer, 0, footer.Length);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Compression.DeflateStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [__DynamicallyInvokable]
        protected override void Dispose(bool disposing)
        {
            try
            {
                this.PurgeBuffers(disposing);
            }
            finally
            {
                try
                {
                    if (disposing)
                    {
                        if (!this._leaveOpen)
                        {
                            if (this._stream != null)
                                this._stream.Close();
                        }
                    }
                }
                finally
                {
                    this._stream = (Stream)null;
                    try
                    {
                        if (this.deflater != null)
                            this.deflater.Dispose();
                    }
                    finally
                    {
                        this.deflater = (IDeflater)null;
                        base.Dispose(disposing);
                    }
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous write operation. (Consider using the <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)"/> method instead; see the Remarks section.)
        /// </summary>
        /// 
        /// <returns>
        /// An  object that represents the asynchronous write operation, which could still be pending.
        /// </returns>
        /// <param name="array">The buffer to write data from.</param><param name="offset">The byte offset in <paramref name="buffer"/> to begin writing from.</param><param name="count">The maximum number of bytes to write.</param><param name="asyncCallback">An optional asynchronous callback, to be called when the write operation is complete.</param><param name="asyncState">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param><exception cref="T:System.IO.IOException">The method tried to write asynchronously past the end of the stream, or a disk error occurred.</exception><exception cref="T:System.ArgumentException">One or more of the arguments is invalid.</exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception><exception cref="T:System.NotSupportedException">The current <see cref="T:System.IO.Compression.DeflateStream"/> implementation does not support the write operation.</exception><exception cref="T:System.InvalidOperationException">The write operation cannot be performed because the stream is closed.</exception>
        [__DynamicallyInvokable]
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            this.EnsureCompressionMode();
            if (this.asyncOperations != 0)
                throw new InvalidOperationException(SR.GetString("InvalidBeginCall"));
            this.ValidateParameters(array, offset, count);
            this.EnsureNotDisposed();
            Interlocked.Increment(ref this.asyncOperations);
            try
            {
                DeflateStreamAsyncResult streamAsyncResult = new DeflateStreamAsyncResult((object)this, asyncState, asyncCallback, array, offset, count);
                streamAsyncResult.isWrite = true;
                this.m_AsyncWriterDelegate.BeginInvoke(array, offset, count, true, this.m_CallBack, (object)streamAsyncResult);
                streamAsyncResult.m_CompletedSynchronously &= streamAsyncResult.IsCompleted;
                return (IAsyncResult)streamAsyncResult;
            }
            catch
            {
                Interlocked.Decrement(ref this.asyncOperations);
                throw;
            }
        }

        private void WriteCallback(IAsyncResult asyncResult)
        {
            DeflateStreamAsyncResult streamAsyncResult = (DeflateStreamAsyncResult)asyncResult.AsyncState;
            streamAsyncResult.m_CompletedSynchronously &= asyncResult.CompletedSynchronously;
            try
            {
                this.m_AsyncWriterDelegate.EndInvoke(asyncResult);
            }
            catch (Exception ex)
            {
                streamAsyncResult.InvokeCallback((object)ex);
                return;
            }
            streamAsyncResult.InvokeCallback((object)null);
        }

        /// <summary>
        /// Ends an asynchronous write operation. (Consider using the <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)"/> method instead; see the Remarks section.)
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Compression.DeflateStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream.</exception><exception cref="T:System.Exception">An exception was thrown during a call to <see cref="M:System.Threading.WaitHandle.WaitOne"/>.</exception><exception cref="T:System.InvalidOperationException">The stream is null.</exception><exception cref="T:System.InvalidOperationException">The end write call is invalid.</exception>
        [__DynamicallyInvokable]
        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.EnsureCompressionMode();
            this.CheckEndXxxxLegalStateAndParams(asyncResult);
            DeflateStreamAsyncResult asyncResult1 = (DeflateStreamAsyncResult)asyncResult;
            this.AwaitAsyncResultCompletion(asyncResult1);
            Exception exception = asyncResult1.Result as Exception;
            if (exception != null)
                throw exception;
        }

        private void CheckEndXxxxLegalStateAndParams(IAsyncResult asyncResult)
        {
            if (this.asyncOperations != 1)
                throw new InvalidOperationException(SR.GetString("InvalidEndCall"));
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            this.EnsureNotDisposed();
            if (!(asyncResult is DeflateStreamAsyncResult))
                throw new ArgumentNullException("asyncResult");
        }

        private void AwaitAsyncResultCompletion(DeflateStreamAsyncResult asyncResult)
        {
            try
            {
                if (asyncResult.IsCompleted)
                    return;
                asyncResult.AsyncWaitHandle.WaitOne();
            }
            finally
            {
                Interlocked.Decrement(ref this.asyncOperations);
                asyncResult.Close();
            }
        }

        internal delegate void AsyncWriteDelegate(byte[] array, int offset, int count, bool isAsync);

        private enum WorkerType : byte
        {
            Managed,
            ZLib,
            Unknown,
        }
    }
}
