﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using AlphaTab.Collections;
using AlphaTab.IO;
using SharpKit.Html;
using SharpKit.JavaScript;

namespace AlphaTab.Platform
{
    public static partial class Std
    {
        public static float ParseFloat(string s)
        {
            return JsContext.parseFloat(s);
        }

        public static int ParseInt(string s)
        {
            return JsContext.parseInt(s);
        }

        [JsMethod(InlineCodeExpression = "dst.set(src.subarray(srcOffset, srcOffset + count), dstOffset)")]
        public static void BlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return s == null || s.Trim().Length == 0;
        }

        [JsMethod(InlineCodeExpression = "String.fromCharCode(c)")]
        public static string StringFromCharCode(int c)
        {
            return "";
        }

        [JsMethod(Code = "for ( var t in e ) { c(e[t]); }")]
        public static void Foreach<T>(IEnumerable<T> e, Action<T> c)
        {
        }

        public static XmlDocument LoadXml(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        public static string GetNodeValue(XmlNode n)
        {
            if (n.NodeType == XmlNodeType.Element || n.NodeType == XmlNodeType.Document)
            {
                var txt = new StringBuilder();
                foreach (XmlNode childNode in n.ChildNodes)
                {
                    txt.Append(GetNodeValue(childNode));
                }
                return txt.ToString().Trim();
            }
            return n.Value;
        }

        public static sbyte ReadSignedByte(this IReadable readable)
        {
            var n = readable.ReadByte();
            if (n >= 128) return (sbyte)(n - 256);
            return (sbyte)n;
        }

        public static string ToString(byte[] data)
        {
            var s = new StringBuilder();
            int i = 0;
            while (i < data.Length)
            {
                var c = data[i++];
                if (c < 0x80)
                {
                    if (c == 0) break;
                    s.AppendChar(c);
                }
                else if (c < 0xE0)
                {
                    s.AppendChar(((c & 0x3F) << 6) | (data[i++] & 0x7F));
                }
                else if (c < 0xF0)
                {
                    s.AppendChar(((c & 0x1F) << 12) | ((data[i++] & 0x7F) << 6) | (data[i++] & 0x7F));
                }
                else
                {
                    var u = ((c & 0x0F) << 18) | ((data[i++] & 0x7F) << 12) |
                            ((data[i++] & 0x7F) << 6) | (data[i++] & 0x7F);
                    s.AppendChar((u >> 18) + 0xD7C0);
                    s.AppendChar((u & 0x3FF) | 0xDC00);
                }
            }
            return s.ToString();
        }

        [JsMethod(InlineCodeExpression = "(value instanceof T)", Export = false)]
        public static bool InstanceOf<T>(object value)
        {
            return true;
        }

        public static byte[] StringToByteArray(string contents)
        {
            var byteArray = new byte[contents.Length];
            for (int i = 0; i < contents.Length; i++)
            {
                byteArray[i] = (byte)contents[i];
            }
            return byteArray;
        }

        [JsMethod(InlineCodeExpression = "new Uint8Array(data)", Export = false)]
        public static byte[] ArrayBufferToByteArray(ArrayBuffer data)
        {
            return null;
        }

        private static string S4()
        {
            JsNumber num = JsMath.floor((1 + JsMath.random()) * 0x10000);
            return num.toString(16).substring(1);
        }

        public static string NewGuid()
        {
            return S4() + S4() + '-' + S4() + '-' + S4() + '-' +
                      S4() + '-' + S4() + S4() + S4();
        }
    }
}
