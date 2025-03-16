
namespace hospital_urgencias.Models;

public class Patient
{
    /// <summary>
    /// Enum representing the patient's status in the hospital.
    /// </summary>
    public enum PatientStatus
    {
        Waiting,
        InConsultation,
        Finished   
    }


    /// <summary>
    /// Represents a patient in the hospital.
    /// </summary>
    public class Patient
    {
        public int Id {get; set;}
        public int HospitalArrival {get; set;}
        public int ConsultationTime {get; set;}
        public PatientStatus Status {get; set;}

        /// <summary>
        /// Initializes a new instance of the patient class.
        /// </summary>
        /// <param name="Id">The unique identification number of the doctor.</param>
        /// <param name="HospitalArrival">The patient's waiting time.</param>
        /// <param name="ConsultationTime">The consultation time the patient needs.</param>
        public Patient (int Id, int HospitalArrival, int ConsultationTime)
        {
            this.Id = Id;
            this.HospitalArrival = HospitalArrival;
            this.ConsultationTime = ConsultationTime;
        }
    }
}
