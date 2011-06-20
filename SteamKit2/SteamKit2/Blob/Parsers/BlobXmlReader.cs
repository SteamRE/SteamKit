using System;
using System.Text;
using System.IO;
using System.Xml;

namespace SteamKit2
{
    public class BlobXmlReader : BlobReader
    {
        private XmlWriter xmlOut;

        public override void Dispose()
        {
            base.Dispose();
            xmlOut.Close();
        }


        private void XmlStartBlob(EAutoPreprocessCode processCode, ECacheState cacheState)
        { 
            xmlOut.WriteStartElement("blob");
        }

        private void XmlEndBlob()
        {
            xmlOut.WriteEndElement();
        }

        private void XmlStartField(FieldKeyType type, byte[] key, int fieldSize)
        {
            string keyValue;

            if (BlobUtil.IsIntDescriptor(key))
                keyValue = Convert.ToString(BitConverter.ToUInt32(key, 0));
            else
                keyValue = Encoding.UTF8.GetString(key);

            xmlOut.WriteStartElement("field");
            xmlOut.WriteAttributeString("key", keyValue);
        }

        private void XmlEndField()
        {
            xmlOut.WriteEndElement();
        }

        private void XmlFieldValue(byte[] data)
        {
            object value = null;
            SuggestedType type = BlobUtil.GetSuggestedType(data);

            switch (type)
            {
                case SuggestedType.StringType:
                    value = BlobUtil.TrimNull(Encoding.ASCII.GetString(data));
                    break;
                case SuggestedType.Int8Type:
                    value = data[0];
                    break;
                case SuggestedType.Int16Type:
                    value = BitConverter.ToInt16(data, 0);
                    break;
                case SuggestedType.Int32Type:
                    value = BitConverter.ToInt32(data, 0);
                    break;
                case SuggestedType.Int64Type:
                    value = BitConverter.ToInt64(data, 0);
                    break;
                default:
                    value = Convert.ToBase64String(data);
                    break;
            }

            xmlOut.WriteAttributeString("type", Convert.ToString(type)); 
            xmlOut.WriteString(Convert.ToString(value));
        }

        private BlobXmlReader(Stream input, Stream output, XmlWriterSettings settings)
            : base(input)
        {
            this.xmlOut = XmlWriter.Create(output, settings);

            Blob += XmlStartBlob;
            EndBlob += XmlEndBlob;
            Field += XmlStartField;
            EndField += XmlEndField;
            FieldValue += XmlFieldValue;
        }


        public static BlobXmlReader Create(Stream inputStream, Stream outputStream, XmlWriterSettings settings)
        {
            return new BlobXmlReader(inputStream, outputStream, settings);
        }

        public static BlobXmlReader Create(Stream inputStream, Stream outputStream)
        {
            return new BlobXmlReader(inputStream, outputStream, null);
        }

        public static BlobXmlReader Create(string fileName, string outFileName, XmlWriterSettings settings)
        {
            return Create(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 0x1000, FileOptions.SequentialScan),
                            new FileStream(outFileName, FileMode.Create), settings);
        }

        public static BlobXmlReader Create(string fileName, string outFileName)
        {
            return Create(fileName, outFileName, null);
        }
    }
}
