﻿// ***********************************************************************
// Assembly         : TessellationAndVoxelizationGeometryLibrary
// Author           : Design Engineering Lab
// Created          : 02-27-2015
//
// Last Modified By : Matt Campbell
// Last Modified On : 05-28-2016
// ***********************************************************************
// <copyright file="IOFunctions.cs" company="Design Engineering Lab">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TVGL.Voxelization;

namespace TVGL.IOFunctions
{
    /// <summary>
    ///     The IO or input/output class contains static functions for saving and loading files in common formats.
    ///     Note that as a Portable class library, these IO functions cannot interact with your file system. In order
    ///     to load or save, the filename is not enough. One needs to provide the stream.
    /// </summary>
    public abstract partial class IO
    {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        internal string Name { get; set; }
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        internal string FileName { get; set; }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>The unit.</value>
        [XmlIgnore]
        public UnitType Units { get; set; }

        /// <summary>
        /// Gets or sets the units as string.
        /// </summary>
        /// <value>The units as string.</value>
        [XmlAttribute("unit")]
        public string UnitsAsString
        {
            get
            {
                return Enum.GetName(typeof(UnitType), Units);
            }
            set
            {
                Units = ParseUnits(value);
            }
        }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        [XmlAttribute("lang")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        internal List<string> Comments => _comments;
        /// <summary>
        /// The _comments
        /// </summary>
        protected List<string> _comments = new List<string>();

        #region Open/Load/Read

#if net40
#else
        /// <summary>
        /// Opens the 3D solid or solids from a provided file name.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>
        /// A list of TessellatedSolids.
        /// </returns>
        /// <exception cref="FileNotFoundException">The file was not found at: " + filename</exception>
        public static void Open(string filename, out TessellatedSolid solid)
        {
            if (File.Exists(filename))
                using (var fileStream = File.OpenRead(filename))
                    Open(fileStream, filename, out solid);
            else throw new FileNotFoundException("The file was not found at: " + filename);
        }
        public static void Open(string filename, out TessellatedSolid[] solids)
        {
            if (File.Exists(filename))
                using (var fileStream = File.OpenRead(filename))
                    Open(fileStream, filename, out solids);
            else throw new FileNotFoundException("The file was not found at: " + filename);
        }
        public static void Open(string filename, out VoxelizedSolid solid)
        {
            if (File.Exists(filename))
                using (var fileStream = File.OpenRead(filename))
                    Open(fileStream, filename, out solid);
            else throw new FileNotFoundException("The file was not found at: " + filename);

        }
        public static void Open(string filename, out VoxelizedSolid[] solids)
        {
            if (File.Exists(filename))
                using (var fileStream = File.OpenRead(filename))
                    Open(fileStream, filename, out solids);
            else throw new FileNotFoundException("The file was not found at: " + filename);
        }
#endif
        /// <summary>
        /// Opens the specified stream, s. Note that as a Portable class library
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="solid">The solid.</param>
        /// <returns>TessellatedSolid.</returns>
        /// <exception cref="Exception">Cannot open file without extension (e.g. f00.stl).
        /// or
        /// This function has been recently removed.
        /// or
        /// Cannot determine format from extension (not .stl, .ply, .3ds, .lwo, .obj, .objx, or .off.</exception>
        /// <exception cref="System.Exception">Cannot open file without extension (e.g. f00.stl).
        /// or
        /// This function has been recently removed.
        /// or
        /// Cannot determine format from extension (not .stl, .ply, .3ds, .lwo, .obj, .objx, or .off.</exception>
        public static void Open(Stream s, string filename, out TessellatedSolid solid)
        {
            var extension = GetExtensionFromFileName(filename);
            switch (extension)
            {
                case "stl":
                    solid = STLFileData.OpenSolids(s, filename)[0]; // Standard Tessellation or StereoLithography
                    break;
                case "3mf":
#if net40
                    throw new NotSupportedException("The loading or saving of .3mf files are not supported in the .NET4.0 version of TVGL.");
#else
                    solid = ThreeMFFileData.OpenSolids(s, filename)[0];
                    break;
#endif
                case "model":
                    solid = ThreeMFFileData.OpenModelFile(s, filename)[0];
                    break;
                case "amf":
                    solid = AMFFileData.OpenSolids(s, filename)[0];
                    break;
                case "off":
                    solid = OFFFileData.OpenSolid(s, filename);
                    // http://en.wikipedia.org/wiki/OFF_(file_format)
                    break;
                case "ply":
                    solid = PLYFileData.OpenSolid(s, filename);
                    break;
                case "shell":
                    solid = ShellFileData.OpenSolids(s, filename)[0];
                    break;
                case "xml":
                    solid = (TessellatedSolid)TVGLFileData.OpenSolids(s, filename)[0];
                    break;
                default:
                    throw new Exception(
                        "Cannot determine format from extension (not .stl, .ply, .3ds, .lwo, .obj, .objx, or .off.");
            }
        }
        public static void Open(Stream s, string filename, out TessellatedSolid[] tessellatedSolids)
        {
            var extension = GetExtensionFromFileName(filename);
            switch (extension)
            {
                case "stl":
                    tessellatedSolids = STLFileData.OpenSolids(s, filename); // Standard Tessellation or StereoLithography
                    break;
                case "3mf":
#if net40
                    throw new NotSupportedException("The loading or saving of .3mf files are not supported in the .NET4.0 version of TVGL.");
#else
                    tessellatedSolids = ThreeMFFileData.OpenSolids(s, filename);
                    break;
#endif
                case "model":
                    tessellatedSolids = ThreeMFFileData.OpenModelFile(s, filename);
                    break;
                case "amf":
                    tessellatedSolids = AMFFileData.OpenSolids(s, filename);
                    break;
                case "shell":
                    tessellatedSolids = ShellFileData.OpenSolids(s, filename);
                    break;
                case "xml":
                    tessellatedSolids = TVGLFileData.OpenSolids(s, filename).Cast<TessellatedSolid>().ToArray();
                    break;
                default:
                    throw new Exception(
                        "Cannot determine format from extension (not .stl, .ply, .3ds, .lwo, .obj, .objx, or .off.");
            }
        }
        public static void Open(Stream s, string filename, out VoxelizedSolid solid)
        {
            solid = (VoxelizedSolid)TVGLFileData.OpenSolids(s, filename)[0];
        }
        public static void Open(Stream s, string filename, out VoxelizedSolid[] solids)
        {
            solids = TVGLFileData.OpenSolids(s, filename).Cast<VoxelizedSolid>().ToArray();
        }


        public static void OpenFromString(string data, FileType fileType, out TessellatedSolid solid)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0;
            var extensions = new[] { "", "STL", "STL", "3mf", "model", "amf", "off", "ply", "ply", "tvgl.xml" };
            var name = "data." + extensions[(int)fileType];
            Open(stream, name, out solid);
        }
        public static void OpenFromString(string data, FileType fileType, out TessellatedSolid[] solids)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0;
            var extensions = new[] { "", "STL", "STL", "3mf", "model", "amf", "off", "ply", "ply", "tvgl.xml" };
            var name = "data." + extensions[(int)fileType];
            Open(stream, name, out solids);
        }
        public static void OpenFromString(string data, FileType fileType, out VoxelizedSolid solid)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0;
            Open(stream, "", out solid);
        }
        public static void OpenFromString(string data, FileType fileType, out VoxelizedSolid[] solids)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0; ;
            Open(stream, "", out solids);
        }

        /// <summary>
        /// Gets the name from the filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.String.</returns>
        protected static string GetNameFromFileName(string filename)
        {
            var startIndex = filename.LastIndexOf('/') + 1;
            if (startIndex == -1) startIndex = filename.LastIndexOf('\\') + 1;
            var endIndex = filename.IndexOf('.', startIndex);
            if (endIndex == -1) endIndex = filename.Length - 1;
            return filename.Substring(startIndex, endIndex - startIndex);
        }
        /// <summary>
        /// Gets the name of the extension from file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.String.</returns>
        protected static string GetExtensionFromFileName(string filename)
        {
            var lastIndexOfDot = filename.LastIndexOf('.');
            if (lastIndexOfDot < 0 || lastIndexOfDot >= filename.Length - 1)
                return "";
            return filename.Substring(lastIndexOfDot + 1).ToLower();
        }


        private static FileType GetFileTypeFromExtension(string extension)
        {
            switch (extension)
            {
                case "stl": return FileType.STL_ASCII;
                case "3mf": return FileType.ThreeMF;
                case "model": return FileType.Model3MF;
                case "amf": return FileType.AMF;
                case "off": return FileType.OFF;
                case "ply": return FileType.PLY_ASCII;
                case "xml": return FileType.TVGL;
                default: return FileType.unspecified;
            }
        }

        private static string GetExtensionFromFileType(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.STL_ASCII:
                case FileType.STL_Binary: return "stl";
                case FileType.ThreeMF: return "3mf";
                case FileType.Model3MF: return "model";
                case FileType.AMF: return "amf";
                case FileType.OFF: return "off";
                case FileType.PLY_ASCII:
                case FileType.PLY_Binary: return "ply";
                case FileType.TVGL: return "xml";
                default: return "";
            }
        }
        /// <summary>
        ///     Parses the ID and values from the specified line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="id">The id.</param>
        /// <param name="values">The values.</param>
        protected static void ParseLine(string line, out string id, out string values)
        {
            line = line.Trim();
            var idx = line.IndexOf(' ');
            if (idx == -1)
            {
                id = line;
                values = String.Empty;
            }
            else
            {
                id = line.Substring(0, idx).ToLower();
                values = line.Substring(idx + 1);
            }
        }


        /// <summary>
        ///     Tries to parse a vertex from a string.
        /// </summary>
        /// <param name="line">The input string.</param>
        /// <param name="doubles">The vertex point.</param>
        /// <returns>True if parsing was successful.</returns>
        protected static bool TryParseDoubleArray(string line, out double[] doubles)
        {
            var strings = line.Split(' ', '\t').ToList();
            strings.RemoveAll(String.IsNullOrWhiteSpace);
            doubles = new double[strings.Count];
            for (var i = 0; i < strings.Count; i++)
            {
                if (!Double.TryParse(strings[i], out doubles[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Tries to parse a vertex from a string.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="line">The input string.</param>
        /// <param name="doubles">The vertex point.</param>
        /// <returns>True if parsing was successful.</returns>
        protected static bool TryParseDoubleArray(Regex parser, string line, out double[] doubles)
        {
            var match = parser.Match(line);
            if (!match.Success)
            {
                doubles = null;
                return false;
            }
            doubles = new double[match.Groups.Count - 1];
            for (var i = 0; i < doubles.GetLength(0); i++)
            {
                if (!Double.TryParse(match.Groups[i + 1].Value, out doubles[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Reads the next line with the expectation that is starts with the
        ///     "expected" string. If the line does not have the expected value,
        ///     then the StreamReader stays at the same position and
        ///     false is returned.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="expected">The expected.</param>
        /// <returns>bool.</returns>
        protected static bool ReadExpectedLine(StreamReader reader, string expected)
        {
            var startPosition = reader.BaseStream.Position;
            var line = ReadLine(reader);
            if (line != null && expected.Equals(line.Trim(' '), StringComparison.CurrentCultureIgnoreCase))
                return true;
            reader.BaseStream.Position = startPosition;
            return false;
        }

        /// <summary>
        ///     Reads the line.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>System.String.</returns>
        protected static string ReadLine(StreamReader reader)
        {
            string line;
            do
            {
                line = reader.ReadLine();
                if (reader.EndOfStream) break;
            } while (String.IsNullOrWhiteSpace(line));
            return line.Trim();
        }

        /// <summary>
        /// Infers the units from comments.
        /// </summary>
        /// <param name="comments">The comments.</param>
        /// <returns></returns>
        protected static UnitType InferUnitsFromComments(List<string> comments)
        {
            UnitType units;
            foreach (var comment in comments)
            {
                var words = Regex.Matches(comment, "([a-z]+)");
                foreach (var word in words)
                    if (TryParseUnits(word.ToString(), out units)) return units;
            }
            return UnitType.unspecified;
        }

        /// <summary>
        /// Tries to parse units.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="units">The units.</param>
        /// <returns></returns>
        protected static bool TryParseUnits(string input, out UnitType units)
        {
            if (Enum.TryParse(input, out units)) return true;
            if (input.Equals("milimeter", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("millimeter", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("milimeters", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("millimeters", StringComparison.CurrentCultureIgnoreCase))
                units = UnitType.millimeter;
            else if (input.Equals("micrometer", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("micrometers", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("microns", StringComparison.CurrentCultureIgnoreCase))
                units = UnitType.micron;
            else if (input.Equals("centimeter", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("centimeters", StringComparison.CurrentCultureIgnoreCase))
                units = UnitType.centimeter;
            else if (input.Equals("meters", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("meter", StringComparison.CurrentCultureIgnoreCase))
                units = UnitType.meter;
            else if (input.Equals("feet", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("foots", StringComparison.CurrentCultureIgnoreCase))
                units = UnitType.foot;
            else if (input.Equals("inches", StringComparison.CurrentCultureIgnoreCase) ||
                input.Equals("inch", StringComparison.CurrentCultureIgnoreCase))
                units = UnitType.inch;
            else return false;
            return true;
        }

        /// <summary>
        /// Parses the units.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>TVGL.UnitType.</returns>
        protected static UnitType ParseUnits(string input)
        {
            UnitType units;
            if (TryParseUnits(input, out units)) return units;
            return UnitType.unspecified;
        }


        internal static int readNumberAsInt(BinaryReader reader, Type type, FormatEndiannessType formatType)
        {
            var bigEndian = (formatType == FormatEndiannessType.binary_big_endian);

            if (type == typeof(double))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return (int)Math.Round(BitConverter.ToDouble(byteArray, 0));
            }
            if (type == typeof(long))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return (int)BitConverter.ToInt64(byteArray, 0);
            }
            if (type == typeof(ulong))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return (int)BitConverter.ToUInt64(byteArray, 0);
            }
            if (type == typeof(float))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return (int)Math.Round(BitConverter.ToSingle(byteArray, 0));
            }
            if (type == typeof(int))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt32(byteArray, 0);
            }
            if (type == typeof(uint))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return (int)BitConverter.ToUInt32(byteArray, 0);
            }
            if (type == typeof(short))
            {
                var byteArray = reader.ReadBytes(2);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt16(byteArray, 0);
            }
            if (type == typeof(ushort))
            {
                var byteArray = reader.ReadBytes(2);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt16(byteArray, 0);
            }
            if (type == typeof(byte))
            {
                var oneByteArray = reader.ReadBytes(1);
                return oneByteArray[0];
            }
            return int.MinValue;
        }
        internal static float readNumberAsFloat(BinaryReader reader, Type type, FormatEndiannessType formatType)
        {
            var bigEndian = (formatType == FormatEndiannessType.binary_big_endian);

            if (type == typeof(double))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return (float)BitConverter.ToDouble(byteArray, 0);
            }
            if (type == typeof(long))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt64(byteArray, 0);
            }
            if (type == typeof(ulong))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt64(byteArray, 0);
            }
            if (type == typeof(float))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToSingle(byteArray, 0);
            }
            if (type == typeof(int))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt32(byteArray, 0);
            }
            if (type == typeof(uint))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt32(byteArray, 0);
            }
            if (type == typeof(short))
            {
                var byteArray = reader.ReadBytes(2);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt16(byteArray, 0);
            }
            if (type == typeof(ushort))
            {
                var byteArray = reader.ReadBytes(2);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt16(byteArray, 0);
            }
            if (type == typeof(byte))
            {
                var oneByteArray = reader.ReadBytes(1);
                return oneByteArray[0];
            }
            return float.NaN;
        }
        internal static double readNumberAsDouble(BinaryReader reader, Type type, FormatEndiannessType formatType)
        {
            var bigEndian = (formatType == FormatEndiannessType.binary_big_endian);

            if (type == typeof(double))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToDouble(byteArray, 0);
            }
            if (type == typeof(long))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt64(byteArray, 0);
            }
            if (type == typeof(ulong))
            {
                var byteArray = reader.ReadBytes(8);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt64(byteArray, 0);
            }
            if (type == typeof(float))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToSingle(byteArray, 0);
            }
            if (type == typeof(int))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt32(byteArray, 0);
            }
            if (type == typeof(uint))
            {
                var byteArray = reader.ReadBytes(4);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt32(byteArray, 0);
            }
            if (type == typeof(short))
            {
                var byteArray = reader.ReadBytes(2);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToInt16(byteArray, 0);
            }
            if (type == typeof(ushort))
            {
                var byteArray = reader.ReadBytes(2);
                if (bigEndian) byteArray = byteArray.Reverse().ToArray();
                return BitConverter.ToUInt16(byteArray, 0);
            }
            if (type == typeof(byte))
            {
                var oneByteArray = reader.ReadBytes(1);
                return oneByteArray[0];
            }
            return double.NaN;
        }


        /// <summary>
        /// Tries the parse number type from string.
        /// </summary>
        /// <param name="typeString">The type string.</param>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected bool TryParseNumberTypeFromString(string typeString, out Type type)
        {
            if (typeString.StartsWith("float", StringComparison.CurrentCultureIgnoreCase)
                || typeString.StartsWith("single", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(float);
            else if (typeString.StartsWith("double", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(double);
            else if (typeString.StartsWith("long", StringComparison.CurrentCultureIgnoreCase)
                || typeString.StartsWith("int64", StringComparison.CurrentCultureIgnoreCase)
                || typeString.StartsWith("uint64", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(long);
            else if (typeString.StartsWith("int32", StringComparison.CurrentCultureIgnoreCase)
                     || typeString.Equals("int", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(int);
            else if (typeString.StartsWith("short", StringComparison.CurrentCultureIgnoreCase)
                     || typeString.StartsWith("int16", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(short);
            else if (typeString.StartsWith("ushort", StringComparison.CurrentCultureIgnoreCase)
                || typeString.StartsWith("uint16", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(ushort);
            else if (typeString.StartsWith("uint64", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(ulong);
            else if (typeString.Equals("uint", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(uint);
            else if (typeString.StartsWith("char", StringComparison.CurrentCultureIgnoreCase)
                     || typeString.StartsWith("uchar", StringComparison.CurrentCultureIgnoreCase)
                     || typeString.StartsWith("byte", StringComparison.CurrentCultureIgnoreCase)
                     || typeString.StartsWith("uint8", StringComparison.CurrentCultureIgnoreCase)
                     || typeString.StartsWith("int8", StringComparison.CurrentCultureIgnoreCase))
                type = typeof(byte);
            else
            {
                type = null;
                return false;
            }
            return true;
        }

        internal static int readNumberAsInt(string text, Type type)
        {
            if (type == typeof(double))
            {
                if (double.TryParse(text, out var value)) return (int)Math.Round(value);
            }
            if (type == typeof(long))
            {
                if (long.TryParse(text, out var value)) return (int)value;
            }
            if (type == typeof(ulong))
            {
                if (ulong.TryParse(text, out var value)) return (int)value;
            }
            if (type == typeof(float))
            {
                if (float.TryParse(text, out var value)) return (int)Math.Round(value);
            }
            if (type == typeof(int))
            {
                if (int.TryParse(text, out var value)) return value;
            }
            if (type == typeof(uint))
            {
                if (uint.TryParse(text, out var value)) return (int)value;
            }
            if (type == typeof(short))
            {
                if (short.TryParse(text, out var value)) return value;
            }
            if (type == typeof(ushort))
            {
                if (ushort.TryParse(text, out var value)) return value;
            }
            if (type == typeof(byte))
            {
                if (byte.TryParse(text, out var value)) return value;
            }
            return int.MinValue;
        }
        internal static float readNumberAsFloat(string text, Type type)
        {
            if (type == typeof(double))
            {
                if (double.TryParse(text, out var value)) return (float)value;
            }
            if (type == typeof(long))
            {
                if (long.TryParse(text, out var value)) return value;
            }
            if (type == typeof(ulong))
            {
                if (ulong.TryParse(text, out var value)) return value;
            }
            if (type == typeof(float))
            {
                if (float.TryParse(text, out var value)) return value;
            }
            if (type == typeof(int))
            {
                if (int.TryParse(text, out var value)) return value;
            }
            if (type == typeof(uint))
            {
                if (uint.TryParse(text, out var value)) return value;
            }
            if (type == typeof(short))
            {
                if (short.TryParse(text, out var value)) return value;
            }
            if (type == typeof(ushort))
            {
                if (ushort.TryParse(text, out var value)) return value;
            }
            if (type == typeof(byte))
            {
                if (byte.TryParse(text, out var value)) return value;
            }
            return float.NaN;
        }
        internal static double readNumberAsDouble(string text, Type type)
        {
            if (type == typeof(double))
            {
                if (Double.TryParse(text, out var value)) return value;
            }
            if (type == typeof(long))
            {
                if (Int64.TryParse(text, out var value)) return value;
            }
            if (type == typeof(ulong))
            {
                if (UInt64.TryParse(text, out var value)) return value;
            }
            if (type == typeof(float))
            {
                if (Single.TryParse(text, out var value)) return value;
            }
            if (type == typeof(int))
            {
                if (Int32.TryParse(text, out var value)) return value;
            }
            if (type == typeof(uint))
            {
                if (UInt32.TryParse(text, out var value)) return value;
            }
            if (type == typeof(short))
            {
                if (Int16.TryParse(text, out var value)) return value;
            }
            if (type == typeof(ushort))
            {
                if (UInt16.TryParse(text, out var value)) return value;
            }
            if (type == typeof(byte))
            {
                if (Byte.TryParse(text, out var value)) return value;
            }
            return Double.NaN;
        }
        #endregion

        #region Save/Write

#if net40
#else
        /// <summary>
        /// Saves the specified solids to a file.
        /// </summary>
        /// <param name="solids">The solids.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns></returns>
        public static bool Save(IList<Solid> solids, string filename, FileType fileType = FileType.unspecified)
        {
            var extension = GetExtensionFromFileName(filename);
            if (fileType == FileType.unspecified)
                fileType = GetFileTypeFromExtension(extension);
            var filenameLocal = filename;
            if (string.IsNullOrWhiteSpace(extension))
                filenameLocal += "." + GetExtensionFromFileType(fileType);
            using (var fileStream = File.OpenWrite(filenameLocal))
                return Save(fileStream, solids, fileType);
        }
        /// <summary>
        /// Saves the specified solid to a file.
        /// </summary>
        /// <param name="solid">The solid.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns></returns>
        public static bool Save(Solid solid, string filename, FileType fileType = FileType.unspecified)
        {
            var extension = GetExtensionFromFileName(filename);
            if (fileType != FileType.unspecified)
                fileType = GetFileTypeFromExtension(extension);
            var filenameLocal = filename;
            if (string.IsNullOrWhiteSpace(extension))
                filenameLocal += "." + GetExtensionFromFileType(fileType);
            using (var fileStream = File.OpenWrite(filenameLocal))
                return Save(fileStream, solid, fileType);
        }
#endif
        /// <summary>
        ///     Saves the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="solids">The solids.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Save(Stream stream, IList<Solid> solids, FileType fileType = FileType.TVGL)
        {
            if (solids.Count == 0) return false;
            if (solids.All(s => s is TessellatedSolid))
            {
                var tessellatedSolids = solids.Cast<TessellatedSolid>().ToArray();
                switch (fileType)
                {
                    case FileType.STL_ASCII:
                        return STLFileData.SaveASCII(stream, tessellatedSolids);
                    case FileType.STL_Binary:
                        return STLFileData.SaveBinary(stream, tessellatedSolids);
                    case FileType.AMF:
                        return AMFFileData.SaveSolids(stream, tessellatedSolids);
                    case FileType.ThreeMF:
#if net40
                    throw new NotSupportedException("The loading or saving of .3mf files are not allowed in the .NET4.0 version of TVGL.");
#else
                        return ThreeMFFileData.Save(stream, tessellatedSolids);
#endif
                    case FileType.Model3MF:
                        return ThreeMFFileData.SaveModel(stream, tessellatedSolids);
                    case FileType.OFF:
                        if (solids.Count > 1)
                            throw new NotSupportedException(
                                "The OFF format does not support saving multiple solids to a single file.");
                        else return OFFFileData.SaveSolid(stream, tessellatedSolids[0]);
                    case FileType.PLY_ASCII:
                        if (solids.Count > 1)
                            throw new NotSupportedException(
                                "The PLY format does not support saving multiple solids to a single file.");
                        else return PLYFileData.SaveSolidASCII(stream, tessellatedSolids[0]);
                    case FileType.PLY_Binary:
                        if (solids.Count > 1)
                            throw new NotSupportedException(
                                "The PLY format does not support saving multiple solids to a single file.");
                        else return PLYFileData.SaveSolidBinary(stream, tessellatedSolids[0]);
                    default:
                        if (solids.Count > 1)
                            return TVGLFileData.SaveSolids(stream, tessellatedSolids);
                        else return TVGLFileData.SaveSolid(stream, tessellatedSolids[0]);
                }
            }

            if (solids.All(s => s is VoxelizedSolid))
            {
                var tessellatedSolids = solids.Cast<VoxelizedSolid>().ToArray();
                if (solids.Count > 1)
                    return TVGLFileData.SaveSolids(stream, solids.Cast<VoxelizedSolid>().ToArray());
                else return TVGLFileData.SaveSolid(stream, (VoxelizedSolid)solids[0]);
            }
            return false;
        }


        /// <summary>
        /// Saves the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="solid">The solid.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns>System.Boolean.</returns>
        public static bool Save(Stream stream, Solid solid, FileType fileType = FileType.TVGL)
        {
            if (solid is TessellatedSolid)
            {
                var ts = (TessellatedSolid)solid;
                switch (fileType)
                {
                    case FileType.STL_ASCII:
                        return STLFileData.SaveASCII(stream, new[] { ts });
                    case FileType.STL_Binary:
                        return STLFileData.SaveBinary(stream, new[] { ts });
                    case FileType.AMF:
                        return AMFFileData.SaveSolids(stream, new[] { ts });
                    case FileType.ThreeMF:
#if net40
                    throw new NotSupportedException("The loading or saving of .3mf files are not allowed in the .NET4.0 version of TVGL.");
#else
                        return ThreeMFFileData.Save(stream, new[] { ts });
#endif
                    case FileType.Model3MF:
                        return ThreeMFFileData.SaveModel(stream, new[] { ts });
                    case FileType.OFF:
                        return OFFFileData.SaveSolid(stream, ts);
                    case FileType.PLY_ASCII:
                        return PLYFileData.SaveSolidASCII(stream, ts);
                    case FileType.PLY_Binary:
                        return PLYFileData.SaveSolidBinary(stream, ts);
                    default:
                        return TVGLFileData.SaveSolid(stream, ts);
                }
            }
            if (solid is VoxelizedSolid) return TVGLFileData.SaveSolid(stream, (VoxelizedSolid)solid);
            return false;
        }


        /// <summary>
        /// Saves the solid as a string.
        /// </summary>
        /// <param name="solid">The solid.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns>System.String.</returns>
        public static string SaveToString(Solid solid, FileType fileType = FileType.unspecified)
        {
            var stream = new MemoryStream();
            if (!Save(stream, solid, fileType)) return "";
            var byteArray = stream.ToArray();
            return System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }

        /// <summary>
        /// Saves the solids as a string.
        /// </summary>
        /// <param name="solids">The solids.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns>System.String.</returns>
        public static string SaveToString(IList<Solid> solids, FileType fileType = FileType.unspecified)
        {
            var stream = new MemoryStream();
            if (!Save(stream, solids, fileType)) return "";
            var byteArray = stream.ToArray();
            return System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }


        /// <summary>
        /// Gets the TVGL date mark text.
        /// </summary>
        /// <value>The TVGL date mark text.</value>
        protected static string tvglDateMarkText
        {
            get
            {
                var now = DateTime.Now;
                return "created by TVGL on " + now.Year + "/" + now.Month + "/" + now.Day + " at " + now.Hour + ":" +
                       now.Minute + ":" + now.Second;
            }
        }
        #endregion
    }
}