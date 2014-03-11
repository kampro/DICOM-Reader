using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace DICOMReader
{
    class Reader
    {
        private gdcm.ImageReader gdcmReader;
        private gdcm.Image image;
        private Bitmap bitmap;
        private System.Windows.Forms.PictureBox pictureBox;

        public Reader(System.Windows.Forms.PictureBox pictureBox)
        {
            this.gdcmReader = new gdcm.ImageReader();
            this.pictureBox = pictureBox;
        }

        public void Read()
        {
            this.gdcmReader.SetFileName(@"D:\CT\IM-0001-0001.dcm");
            //this.gdcmReader.SetFileName(@"D:\Programy\VS\DICOMReader\Grassroots-DICOM\SIEMENS_MOSAIC_12BitsStored-16BitsJPEG.dcm");

            if (this.gdcmReader.Read())
            {
                this.image = this.gdcmReader.GetImage();
                this.ReadToBitmap();
            }
        }

        private void ReadToBitmap()
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
                        else
                        {
                            if (tempUInt16 < minLuminance)
                                minLuminance = tempUInt16;
                        }
                    }

                    double luminanceScale = 255d / maxLuminance;
                    double tempDouble;
                    // DEBUG
                    System.Diagnostics.Debug.WriteLine("min luminance:\t{0}", minLuminance);
                    System.Diagnostics.Debug.WriteLine("max luminance:\t{0}", maxLuminance);
                    System.Diagnostics.Debug.WriteLine("luminance scale:\t{0}", luminanceScale);

                    for (int i = 0, j = 0; i < bufferConverted.Length; i++, j += 3)
                    {
                        tempDouble = Math.Floor(bufferConverted[i] * luminanceScale);

                        if (tempDouble > 255)
                            tempDouble = 255;

                        buffer[j] = (byte)tempDouble;
                        buffer[j + 1] = buffer[j];
                        buffer[j + 2] = buffer[j];
                    }

                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);
                }
                finally
                {
                    this.bitmap.UnlockBits(bitmapData);
                }

                this.pictureBox.Image = this.bitmap;
            }
        }
    }
}
