/*
This code is licenced under MIT

// The MIT License
//
// Copyright (c) 2006-2008 TinyVine Software Limited.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

Portions of this software are Copyright of Simone Chiaretta
Portions of this software are Copyright of Nate Kohari
Portions of this software are Copyright of Alex Henderson
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace SteamKit2
{
    [Serializable]
    sealed class BerDecodeException : Exception
    {
        readonly int _position;

        public BerDecodeException()
        {
        }

        public BerDecodeException(String message)
            : base(message)
        {
        }

        public BerDecodeException(String message, Exception ex)
            : base(message, ex)
        {
        }

        public BerDecodeException(String message, int position)
            : base(message)
        {
            _position = position;
        }

        public BerDecodeException(String message, int position, Exception ex)
            : base(message, ex)
        {
            _position = position;
        }

        BerDecodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _position = info.GetInt32("Position");
        }

        public int Position
        {
            get { return _position; }
        }

        public override string Message
        {
            get
            {
                var sb = new StringBuilder(base.Message);

                sb.AppendFormat(" (Position {0}){1}",
                                _position, Environment.NewLine);

                return sb.ToString();
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Position", _position);
        }
    }
    
    
    class AsnKeyParser
    {
            readonly AsnParser _parser;

            public AsnKeyParser(String pathname)
            {
                using (var reader = new BinaryReader(
                  new FileStream(pathname, FileMode.Open, FileAccess.Read)))
                {
                    var info = new FileInfo(pathname);

                    _parser = new AsnParser(reader.ReadBytes((int)info.Length));
                }
            }

            public AsnKeyParser(ICollection<byte> contents)
            {
                _parser = new AsnParser(contents);
            }

            public static byte[] TrimLeadingZero(byte[] values)
            {
                byte[] r;
                if ((0x00 == values[0]) && (values.Length > 1))
                {
                    r = new byte[values.Length - 1];
                    Array.Copy(values, 1, r, 0, values.Length - 1);
                }
                else
                {
                    r = new byte[values.Length];
                    Array.Copy(values, r, values.Length);
                }

                return r;
            }

            public static bool EqualOid(byte[] first, byte[] second)
            {
                if (first.Length != second.Length)
                {
                    return false;
                }

                for (int i = 0; i < first.Length; i++)
                {
                    if (first[i] != second[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public RSAParameters ParseRSAPublicKey()
            {
                var parameters = new RSAParameters();

                // Current value

                // Sanity Check

                // Checkpoint
                int position = _parser.CurrentPosition();

                // Ignore Sequence - PublicKeyInfo
                int length = _parser.NextSequence();
                if (length != _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect Sequence Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    _parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                // Checkpoint
                position = _parser.CurrentPosition();

                // Ignore Sequence - AlgorithmIdentifier
                length = _parser.NextSequence();
                if (length > _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect AlgorithmIdentifier Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    _parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                // Checkpoint
                position = _parser.CurrentPosition();
                // Grab the OID
                byte[] value = _parser.NextOID();
                byte[] oid = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
                if (!EqualOid(value, oid))
                {
                    throw new BerDecodeException("Expected OID 1.2.840.113549.1.1.1", position);
                }

                // Optional Parameters
                if (_parser.IsNextNull())
                {
                    _parser.NextNull();
                    // Also OK: value = _parser.Next();
                }
                else
                {
                    // Gracefully skip the optional data
                    _parser.Next();
                }

                // Checkpoint
                position = _parser.CurrentPosition();

                // Ignore BitString - PublicKey
                length = _parser.NextBitString();
                if (length > _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect PublicKey Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    (_parser.RemainingBytes()).ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                // Checkpoint
                position = _parser.CurrentPosition();

                // Ignore Sequence - RSAPublicKey
                length = _parser.NextSequence();
                if (length < _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect RSAPublicKey Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    _parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                parameters.Modulus = TrimLeadingZero(_parser.NextInteger());
                parameters.Exponent = TrimLeadingZero(_parser.NextInteger());

                Debug.Assert(0 == _parser.RemainingBytes());

                return parameters;
            }

            public DSAParameters ParseDSAPublicKey()
            {
                var parameters = new DSAParameters();

                // Current value

                // Current Position
                int position = _parser.CurrentPosition();
                // Sanity Checks

                // Ignore Sequence - PublicKeyInfo
                int length = _parser.NextSequence();
                if (length != _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect Sequence Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    _parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                // Checkpoint
                position = _parser.CurrentPosition();

                // Ignore Sequence - AlgorithmIdentifier
                length = _parser.NextSequence();
                if (length > _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect AlgorithmIdentifier Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    _parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                // Checkpoint
                position = _parser.CurrentPosition();

                // Grab the OID
                byte[] value = _parser.NextOID();
                byte[] oid = { 0x2a, 0x86, 0x48, 0xce, 0x38, 0x04, 0x01 };
                if (!EqualOid(value, oid))
                {
                    throw new BerDecodeException("Expected OID 1.2.840.10040.4.1", position);
                }


                // Checkpoint
                position = _parser.CurrentPosition();

                // Ignore Sequence - DSS-Params
                length = _parser.NextSequence();
                if (length > _parser.RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect DSS-Params Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    length.ToString(CultureInfo.InvariantCulture),
                                    _parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                // Next three are curve parameters
                parameters.P = TrimLeadingZero(_parser.NextInteger());
                parameters.Q = TrimLeadingZero(_parser.NextInteger());
                parameters.G = TrimLeadingZero(_parser.NextInteger());

                // Ignore BitString - PrivateKey
                _parser.NextBitString();

                // Public Key
                parameters.Y = TrimLeadingZero(_parser.NextInteger());

                Debug.Assert(0 == _parser.RemainingBytes());

                return parameters;
            }
    }

    internal class AsnParser
        {
            readonly int _initialCount;
            readonly List<byte> _octets;

            public AsnParser(ICollection<byte> values)
            {
                _octets = new List<byte>(values.Count);
                _octets.AddRange(values);

                _initialCount = _octets.Count;
            }

            public int CurrentPosition()
            {
                return _initialCount - _octets.Count;
            }

            public int RemainingBytes()
            {
                return _octets.Count;
            }

            int GetLength()
            {
                int length = 0;

                // Checkpoint
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();

                    if (b == (b & 0x7f))
                    {
                        return b;
                    }
                    int i = b & 0x7f;

                    if (i > 4)
                    {
                        var sb = new StringBuilder("Invalid Length Encoding. ");
                        sb.AppendFormat("Length uses {0} _octets",
                                        i.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    while (0 != i--)
                    {
                        // shift left
                        length <<= 8;

                        length |= GetNextOctet();
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }

                return length;
            }

            public byte[] Next()
            {
                int position = CurrentPosition();

                try
                {
#pragma warning disable 168
#pragma warning disable 219
                    byte b = GetNextOctet();
#pragma warning restore 219
#pragma warning restore 168

                    int length = GetLength();
                    if (length > RemainingBytes())
                    {
                        var sb = new StringBuilder("Incorrect Size. ");
                        sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                        length.ToString(CultureInfo.InvariantCulture),
                                        RemainingBytes().ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    return GetOctets(length);
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }

            public byte GetNextOctet()
            {
                int position = CurrentPosition();

                if (0 == RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    1.ToString(CultureInfo.InvariantCulture),
                                    RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                byte b = GetOctets(1)[0];

                return b;
            }

            public byte[] GetOctets(int octetCount)
            {
                int position = CurrentPosition();

                if (octetCount > RemainingBytes())
                {
                    var sb = new StringBuilder("Incorrect Size. ");
                    sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                    octetCount.ToString(CultureInfo.InvariantCulture),
                                    RemainingBytes().ToString(CultureInfo.InvariantCulture));
                    throw new BerDecodeException(sb.ToString(), position);
                }

                var values = new byte[octetCount];

                try
                {
                    _octets.CopyTo(0, values, 0, octetCount);
                    _octets.RemoveRange(0, octetCount);
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }

                return values;
            }

            public bool IsNextNull()
            {
                return 0x05 == _octets[0];
            }

            public int NextNull()
            {
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();
                    if (0x05 != b)
                    {
                        var sb = new StringBuilder("Expected Null. ");
                        sb.AppendFormat("Specified Identifier: {0}", b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    // Next octet must be 0
                    b = GetNextOctet();
                    if (0x00 != b)
                    {
                        var sb = new StringBuilder("Null has non-zero size. ");
                        sb.AppendFormat("Size: {0}", b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    return 0;
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }

            public bool IsNextSequence()
            {
                return 0x30 == _octets[0];
            }

            public int NextSequence()
            {
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();
                    if (0x30 != b)
                    {
                        var sb = new StringBuilder("Expected Sequence. ");
                        sb.AppendFormat("Specified Identifier: {0}",
                                        b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    int length = GetLength();
                    if (length > RemainingBytes())
                    {
                        var sb = new StringBuilder("Incorrect Sequence Size. ");
                        sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                        length.ToString(CultureInfo.InvariantCulture),
                                        RemainingBytes().ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    return length;
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }

            public bool IsNextOctetString()
            {
                return 0x04 == _octets[0];
            }

            public int NextOctetString()
            {
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();
                    if (0x04 != b)
                    {
                        var sb = new StringBuilder("Expected Octet String. ");
                        sb.AppendFormat("Specified Identifier: {0}", b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    int length = GetLength();
                    if (length > RemainingBytes())
                    {
                        var sb = new StringBuilder("Incorrect Octet String Size. ");
                        sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                        length.ToString(CultureInfo.InvariantCulture),
                                        RemainingBytes().ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    return length;
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }

            public bool IsNextBitString()
            {
                return 0x03 == _octets[0];
            }

            public int NextBitString()
            {
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();
                    if (0x03 != b)
                    {
                        var sb = new StringBuilder("Expected Bit String. ");
                        sb.AppendFormat("Specified Identifier: {0}", b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    int length = GetLength();

                    // We need to consume unused bits, which is the first
                    //   octet of the remaing values
                    b = _octets[0];
                    _octets.RemoveAt(0);
                    length--;

                    if (0x00 != b)
                    {
                        throw new BerDecodeException("The first octet of BitString must be 0", position);
                    }

                    return length;
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }

            public bool IsNextInteger()
            {
                return 0x02 == _octets[0];
            }

            public byte[] NextInteger()
            {
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();
                    if (0x02 != b)
                    {
                        var sb = new StringBuilder("Expected Integer. ");
                        sb.AppendFormat("Specified Identifier: {0}", b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    int length = GetLength();
                    if (length > RemainingBytes())
                    {
                        var sb = new StringBuilder("Incorrect Integer Size. ");
                        sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                        length.ToString(CultureInfo.InvariantCulture),
                                        RemainingBytes().ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    return GetOctets(length);
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }

            public byte[] NextOID()
            {
                int position = CurrentPosition();

                try
                {
                    byte b = GetNextOctet();
                    if (0x06 != b)
                    {
                        var sb = new StringBuilder("Expected Object Identifier. ");
                        sb.AppendFormat("Specified Identifier: {0}",
                                        b.ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    int length = GetLength();
                    if (length > RemainingBytes())
                    {
                        var sb = new StringBuilder("Incorrect Object Identifier Size. ");
                        sb.AppendFormat("Specified: {0}, Remaining: {1}",
                                        length.ToString(CultureInfo.InvariantCulture),
                                        RemainingBytes().ToString(CultureInfo.InvariantCulture));
                        throw new BerDecodeException(sb.ToString(), position);
                    }

                    var values = new byte[length];

                    for (int i = 0; i < length; i++)
                    {
                        values[i] = _octets[0];
                        _octets.RemoveAt(0);
                    }

                    return values;
                }

                catch (ArgumentOutOfRangeException ex)
                {
                    throw new BerDecodeException("Error Parsing Key", position, ex);
                }
            }
        }
 
}
