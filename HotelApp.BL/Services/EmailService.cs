using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using HotelApp.BL.Dtos;
using HotelApp.BL.Interfaces;
using HotelApp.BL.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HotelApp.BL.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.SenderEmail))
            {
                _logger.LogWarning("SMTP is not configured; email '{Subject}' was not sent to {Recipient}.", subject, to);
                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_options.SenderEmail, _options.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                using var smtp = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.UseSsl
                };

                if (!string.IsNullOrWhiteSpace(_options.UserName))
                    smtp.Credentials = new NetworkCredential(_options.UserName, _options.Password);

                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send email '{Subject}' to {Recipient}.", subject, to);
                return false;
            }
        }

        // RF-013
        public Task<bool> SendReservationConfirmationAsync(FullReservationDto reservation)
        {
            var subject = $"Confirmación de reserva {reservation.Reservation.ReservationCode}";

            var body = new StringBuilder();
            body.Append($"<h2>¡Gracias por reservar con nosotros, {Encode(reservation.Guest.FirstName)}!</h2>");
            body.Append($"<p>Su código de reserva es <strong>{Encode(reservation.Reservation.ReservationCode)}</strong>.</p>");
            body.Append(StaySummary(reservation));
            body.Append("<p>El Check-in es a partir de las 3:00 PM y el Check-out hasta las 12:00 del mediodía.</p>");

            return SendAsync(reservation.Guest.Email, subject, body.ToString());
        }

        // RF-014
        public Task<bool> SendReservationCancellationAsync(FullReservationDto reservation, decimal penalty)
        {
            var subject = $"Cancelación de reserva {reservation.Reservation.ReservationCode}";

            var body = new StringBuilder();
            body.Append($"<h2>Su reserva fue cancelada, {Encode(reservation.Guest.FirstName)}</h2>");
            body.Append($"<p>La reserva <strong>{Encode(reservation.Reservation.ReservationCode)}</strong> quedó cancelada.</p>");
            body.Append(StaySummary(reservation));

            if (penalty > 0)
                body.Append($"<p>Por cancelar el mismo día de la llegada se aplicó un cargo de <strong>RD$ {penalty:N2}</strong> (100% de la primera noche).</p>");
            else
                body.Append("<p>No se aplicó ningún cargo por la cancelación.</p>");

            return SendAsync(reservation.Guest.Email, subject, body.ToString());
        }

        private static string StaySummary(FullReservationDto reservation)
        {
            var sb = new StringBuilder();
            sb.Append($"<p>Llegada: <strong>{reservation.Reservation.CheckInDate:dd/MM/yyyy}</strong> &nbsp; Salida: <strong>{reservation.Reservation.CheckOutDate:dd/MM/yyyy}</strong> ({reservation.Nights} noche(s))</p>");
            sb.Append("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\"><tr><th>Habitación</th><th>Tipo</th><th>Precio por noche</th></tr>");

            foreach (var room in reservation.Rooms)
            {
                sb.Append($"<tr><td>{Encode(room.RoomNumber)}</td><td>{Encode(room.RoomType)}</td><td>RD$ {room.PricePerNight:N2}</td></tr>");
            }

            sb.Append("</table>");
            sb.Append($"<p>Total de la reserva: <strong>RD$ {reservation.Reservation.Total:N2}</strong></p>");
            return sb.ToString();
        }

        private static string Encode(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);
    }
}
