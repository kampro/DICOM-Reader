using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DICOMReader
{
    class Reader
    {
        private gdcm.ImageReader gdcmReader;
        private gdcm.Image image;
        private gdcm.File file;
        private Bitmap bitmap;
        private FileInformation fileInformation;
        private string filePath;

        public Bitmap BitmapImage
        {
            get { return this.bitmap; }
        }

        public FileInformation FileInf
        {
            get { return this.fileInformation; }
        }

        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }

        public Reader()
        {
        }

        public bool Read()
        {
            this.gdcmReader = new gdcm.ImageReader();
            this.gdcmReader.SetFileName(this.filePath);

            if (this.gdcmReader.Read())
            {
                this.file = this.gdcmReader.GetFile();
                this.image = this.gdcmReader.GetImage();
                this.ReadToBitmap();
                this.ReadTags();

                return true;
            }
            else
                return false;
        }

        public string ListAllTags()
        {
            if (this.image != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(this.file.GetHeader().toString());
                //stringBuilder.AppendLine(this.file.GetDataSet().toString());

                gdcm.CSharpDataSet csDataSet = new gdcm.CSharpDataSet(this.file.GetDataSet());
                gdcm.DataElement dataElement;
                gdcm.Tag tag;
                gdcm.VR vr;
                uint uvl;

                while (!csDataSet.IsAtEnd())
                {
                    dataElement = csDataSet.GetCurrent();

                    tag = dataElement.GetTag();
                    vr = gdcm.DataSetHelper.ComputeVR(this.file, this.file.GetDataSet(), tag);

                    if (vr.Compatible(new gdcm.VR(gdcm.VR.VRType.SQ)))
                    {
                        uvl = (uint)dataElement.GetVL();

                        stringBuilder.Append(tag.toString());
                        stringBuilder.Append("\t:\t");
                        stringBuilder.AppendLine(uvl.ToString());
                    }
                    else
                    {
                        //stringBuilder.Append(tag.toString());
                        //stringBuilder.Append(" : ");
                        stringBuilder.AppendLine(dataElement/*.GetValue()*/.toString());
                    }

                    csDataSet.Next();
                }

                stringBuilder.Replace("\n", "\r\n");

                return stringBuilder.ToString();
            }
            else
                return "DICOM file is not selected";
        }

        private bool ReadToBitmap()
        {
            if (this.image != null)
            {
                int dimX = (int)this.image.GetDimension(0);
                int dimY = (int)this.image.GetDimension(1);
                // DEBUG
                System.Diagnostics.Debug.WriteLine("dimensions:\t{0}x{1}", dimX, dimY);
                System.Diagnostics.Debug.WriteLine("pixel format:\t" + this.image.GetPixelFormat().GetScalarType().ToString());

                this.bitmap = new Bitmap(dimX, dimY);
                BitmapData bitmapData = this.bitmap.LockBits(new Rectangle(0, 0, dimX, dimY), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                try
                {
                    byte[] buffer16 = new byte[this.image.GetBufferLength()];
                    this.image.GetBuffer(buffer16);

                    byte[] buffer = new byte[(int)(buffer16.Length * 1.5)]; // 24-bit buffer
                    UInt16[] bufferConverted = new UInt16[buffer16.Length / 2];

                    int maxLuminance = 0;
                    int minLuminance = 0;
                    UInt16 tempUInt16;

                    for (int i = 0, j = 0; i < buffer16.Length; i += 2, j++)
                    {
                        tempUInt16 = BitConverter.ToUInt16(buffer16, i);
                        bufferConverted[j] = tempUInt16;

                        if (tempUInt16 > maxLuminance)
                            maxLuminance = tempUInt16;

                        if (i == 0)
                            minLuminance = tempUInt16;
                        //else
                        //{
                        //    if (tempUInt16 < minLuminance && tempUInt16 != 0)
                        //        minLuminance = tempUInt16;
                        //}
                    }

                    double luminanceScale = 255d / (maxLuminance - minLuminance);
                    double tempDouble;
                    // DEBUG
                    System.Diagnostics.Debug.WriteLine("min luminance:\t{0}", minLuminance);
                    System.Diagnostics.Debug.WriteLine("max luminance:\t{0}", maxLuminance);
                    System.Diagnostics.Debug.WriteLine("luminance scale:\t{0}", luminanceScale);

                    for (int i = 0, j = 0; i < bufferConverted.Length; i++, j += 3)
                    {
                        tempDouble = Math.Floor((bufferConverted[i] - minLuminance) * luminanceScale);

                        if (tempDouble > 255)
                            tempDouble = 255;
                        else if (tempDouble < 0)
                            tempDouble = 0;

                        buffer[j] = (byte)tempDouble;
                        buffer[j + 1] = buffer[j];
                        buffer[j + 2] = buffer[j];
                    }

                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    this.bitmap.UnlockBits(bitmapData);
                }

                return true;
            }
            else
                return false;
        }

        private bool ReadTags()
        {
            if (this.image != null)
            {
                this.fileInformation = new FileInformation();

                this.fileInformation.StudyID = this.ReadTag(0x20, 0x10);
                this.fileInformation.StudyInstanceUID = this.ReadTag(0x20, 0xD);
                this.fileInformation.PatientID = this.ReadTag(0x10, 0x20);
                this.fileInformation.PatientName = this.ReadTag(0x10, 0x10);
                this.fileInformation.PatientBirthDate = this.ReadTag(0x10, 0x30);
                this.fileInformation.PatientAge = this.ReadTag(0x10, 0x1010);
                this.fileInformation.BodyPart = this.ReadTag(0x18, 0x15);
                this.fileInformation.ProcedureStep = this.ReadTag(0x40, 0x254);
                this.fileInformation.PatientSex = this.ReadTag(0x10, 0x40);
                this.fileInformation.PatientWeight = this.ReadTag(0x10, 0x1030);
                this.fileInformation.PatientSize = this.ReadTag(0x10, 0x1020);
                this.fileInformation.PatientPosition = this.ReadTag(0x18, 0x5100);
                this.fileInformation.PatientOrientation = this.ReadTag(0x20, 0x20);
                this.fileInformation.PatientMotherName = this.ReadTag(0x10, 0x1060);
                this.fileInformation.PatientReligion = this.ReadTag(0x10, 0x21F0);
                this.fileInformation.PatientAddress = this.ReadTag(0x10, 0x1040);
                this.fileInformation.AcquisitionDate = this.ReadTag(0x8, 0x22);
                this.fileInformation.ContentDate = this.ReadTag(0x8, 0x23);
                this.fileInformation.ContentTime = this.ReadTag(0x8, 0x33);
                this.fileInformation.CreationDate = this.ReadTag(0x2100, 0x40);
                this.fileInformation.CreationTime = this.ReadTag(0x2100, 0x50);
                this.fileInformation.Date = this.ReadTag(0x40, 0xA121);
                this.fileInformation.SeriesDate = this.ReadTag(0x8, 0x21);
                this.fileInformation.SeriesTime = this.ReadTag(0x8, 0x31);
                this.fileInformation.StudyDate = this.ReadTag(0x8, 0x20);
                this.fileInformation.StudyTime = this.ReadTag(0x8, 0x30);
                this.fileInformation.TreatmentDate = this.ReadTag(0x3008, 0x250);
                this.fileInformation.ImageID = this.ReadTag(0x54, 0x400);
                this.fileInformation.ImageCenter = this.ReadTag(0x28, 0x1050);
                this.fileInformation.ImageOrientation = this.ReadTag(0x20, 0x37);
                this.fileInformation.ImagePosition = this.ReadTag(0x2020, 0x10);
                this.fileInformation.ImageRotation = this.ReadTag(0x70, 0x42);
                this.fileInformation.InstitutionName = this.ReadTag(0x8, 0x80);
                this.fileInformation.InstitutionAddress = this.ReadTag(0x8, 0x81);
                this.fileInformation.DeviceCalibrationDate = this.ReadTag(0x18, 0x1200);
                this.fileInformation.DeviceManufacturer = this.ReadTag(0x8, 0x70);
                this.fileInformation.DeviceModel = this.ReadTag(0x8, 0x1090);
                this.fileInformation.DetectorID = this.ReadTag(0x18, 0x700A);
                this.fileInformation.SOPClassUID = this.ReadTag(0x8, 0x16);
                this.fileInformation.SOPInstanceUID = this.ReadTag(0x8, 0x18);

                return true;
            }
            else
                return false;
        }

        private string ReadTag(ushort group, ushort element)
        {
            gdcm.Tag tag = new gdcm.Tag(group, element);

            if (group == 0x2)
            {
                gdcm.FileMetaInformation fileMetaInfo = this.file.GetHeader();

                if (fileMetaInfo.FindDataElement(tag))
                    return fileMetaInfo.GetDataElement(tag).GetValue().toString();
                else
                    return string.Empty;
            }
            else
            {
                gdcm.DataSet dataSet = this.file.GetDataSet();

                if (dataSet.FindDataElement(tag))
                    return dataSet.GetDataElement(tag).GetValue().toString();
                else
                    return string.Empty;
            }
        }
    }
}
