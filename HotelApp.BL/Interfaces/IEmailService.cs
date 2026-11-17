using System.Threading.Tasks;
using HotelApp.BL.Dtos;

namespace HotelApp.BL.Interfaces
{
    /// <summary>
    /// CU-015. None of these methods throw: if the email is not sent, it is logged
    /// and false is returned, so an email failure never breaks an already saved reservation.
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendAsync(string to, string subject, string htmlBody);

        /// <summary>RF-013: confirmation email when a reservation is created.</summary>
        Task<bool> SendReservationConfirmationAsync(FullReservationDto reservation);

        /// <summary>RF-014: email when a reservation is cancelled (includes the penalty if any, RN-008).</summary>
        Task<bool> SendReservationCancellationAsync(FullReservationDto reservation, decimal penalty);
    }
}
