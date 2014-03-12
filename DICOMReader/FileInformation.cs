using System;
using System.Collections.Generic;

namespace DICOMReader
{
    class FileInformation
    {
        public string StudyID { get; set; }
        public string StudyInstanceUID { get; set; }
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string PatientBirthDate { get; set; }
        public string PatientAge { get; set; }
        public string BodyPart { get; set; }
        public string ProcedureStep { get; set; }
        public string PatientSex { get; set; }
        public string PatientWeight { get; set; }
        public string PatientSize { get; set; }
        public string PatientPosition { get; set; }
        public string PatientOrientation { get; set; }
        public string PatientMotherName { get; set; }
        public string PatientReligion { get; set; }
        public string PatientAddress { get; set; }
        public string AcquisitionDate { get; set; }
        public string ContentDate { get; set; }
        public string ContentTime { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
        public string Date { get; set; }
        public string SeriesDate { get; set; }
        public string SeriesTime { get; set; }
        public string StudyDate { get; set; }
        public string StudyTime { get; set; }
        public string TreatmentDate { get; set; }
        public string ImageID { get; set; }
        public string ImageCenter { get; set; }
        public string ImageOrientation { get; set; }
        public string ImagePosition { get; set; }
        public string ImageRotation { get; set; }
        public string InstitutionName { get; set; }
        public string InstitutionAddress { get; set; }
        public string DeviceCalibrationDate { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceModel { get; set; }
        public string DetectorID { get; set; }
        public string SOPClassUID { get; set; }
        public string SOPInstanceUID { get; set; }

        public Dictionary<string, string> GetTagsDictionary()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            dictionary.Add("Study ID", this.StudyID);
            dictionary.Add("Study instance UID", this.StudyInstanceUID);
            dictionary.Add("Patient ID", this.PatientID);
            dictionary.Add("Patient's name", this.PatientName);
            dictionary.Add("Patient's birth date", this.PatientBirthDate);
            dictionary.Add("Patient's age", this.PatientAge);
            dictionary.Add("Body part", this.BodyPart);
            dictionary.Add("Procedure step", this.ProcedureStep);
            dictionary.Add("Patient's sex", this.PatientSex);
            dictionary.Add("Patient's weight", this.PatientWeight);
            dictionary.Add("Patient's size", this.PatientSize);
            dictionary.Add("Patient's position", this.PatientPosition);
            dictionary.Add("Patient's orientation", this.PatientOrientation);
            dictionary.Add("Patient's mother's name", this.PatientMotherName);
            dictionary.Add("Patient's religion", this.PatientReligion);
            dictionary.Add("Patient's address", this.PatientAddress);
            dictionary.Add("Acquisition date", this.AcquisitionDate);
            dictionary.Add("Content date", this.ContentDate);
            dictionary.Add("Content time", this.ContentTime);
            dictionary.Add("Creation date", this.CreationDate);
            dictionary.Add("Creation time", this.CreationTime);
            dictionary.Add("Date", this.Date);
            dictionary.Add("Series date", this.SeriesDate);
            dictionary.Add("Series time", this.SeriesTime);
            dictionary.Add("Study date", this.StudyDate);
            dictionary.Add("Study time", this.StudyTime);
            dictionary.Add("Treatment date", this.TreatmentDate);
            dictionary.Add("Image ID", this.ImageID);
            dictionary.Add("Image center", this.ImageCenter);
            dictionary.Add("Image orientation", this.ImageOrientation);
            dictionary.Add("Image position", this.ImagePosition);
            dictionary.Add("Image rotation", this.ImageRotation);
            dictionary.Add("Institution name", this.InstitutionName);
            dictionary.Add("Institution address", this.InstitutionAddress);
            dictionary.Add("Device calibration date", this.DeviceCalibrationDate);
            dictionary.Add("Device manufacturer", this.DeviceManufacturer);
            dictionary.Add("Device model", this.DeviceModel);
            dictionary.Add("Detector ID", this.DetectorID);
            dictionary.Add("SOP class UID", this.SOPClassUID);
            dictionary.Add("SOP instance UID", this.SOPInstanceUID);

            return dictionary;
        }
    }
}
