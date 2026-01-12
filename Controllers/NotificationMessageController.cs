using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Enums;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationMessageController : ControllerBase
    {
        private readonly INotificationMessageRepository _notificationMessageRepository;
        private readonly IMapper _mapper;

        public NotificationMessageController(INotificationMessageRepository notificationMessageRepository, IMapper mapper)
        {
            _notificationMessageRepository = notificationMessageRepository;
            _mapper = mapper;
        }

        // GET: api/notificationmessage
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationMessageReadDTO>>> GetAllNotifications()
        {
            var notifications = await _notificationMessageRepository.GetAllNotificationsAsync();
            var dtos = _mapper.Map<IEnumerable<NotificationMessageReadDTO>>(notifications);
            return Ok(dtos);
        }

        // GET: api/notificationmessage/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationMessageReadDTO>> GetNotificationById(int id)
        {
            var notification = await _notificationMessageRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<NotificationMessageReadDTO>(notification);
            return Ok(dto);
        }

        // GET: api/notificationmessage/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationMessageReadDTO>>> GetNotificationsByUserId(int userId)
        {
            var notifications = await _notificationMessageRepository.GetNotificationsByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<NotificationMessageReadDTO>>(notifications);
            return Ok(dtos);
        }

        // GET: api/notificationmessage/user/{userId}/unread
        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<NotificationMessageReadDTO>>> GetUnreadNotificationsByUserId(int userId)
        {
            var notifications = await _notificationMessageRepository.GetUnreadNotificationsByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<NotificationMessageReadDTO>>(notifications);
            return Ok(dtos);
        }

        // GET: api/notificationmessage/type/{type}
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<NotificationMessageReadDTO>>> GetNotificationsByType(NotificationType type)
        {
            var notifications = await _notificationMessageRepository.GetNotificationsByTypeAsync(type);
            var dtos = _mapper.Map<IEnumerable<NotificationMessageReadDTO>>(notifications);
            return Ok(dtos);
        }

        // POST: api/notificationmessage
        [HttpPost]
        public async Task<ActionResult<NotificationMessageReadDTO>> CreateNotification([FromBody] NotificationMessageWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = _mapper.Map<NotificationMessage>(writeDTO);
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            var createdNotification = await _notificationMessageRepository.AddNotificationAsync(notification);
            await _notificationMessageRepository.CompleteAsync();

            var readDTO = _mapper.Map<NotificationMessageReadDTO>(createdNotification);
            return CreatedAtAction(nameof(GetNotificationById), new { id = createdNotification.Id }, readDTO);
        }

        // PUT: api/notificationmessage/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, [FromBody] NotificationMessageWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationMessageRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _mapper.Map(writeDTO, notification);
            notification.UpdatedAt = DateTime.UtcNow;

            _notificationMessageRepository.UpdateNotification(notification);
            await _notificationMessageRepository.CompleteAsync();

            return NoContent();
        }

        // PATCH: api/notificationmessage/{id}/mark-as-read
        [HttpPatch("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationMessageRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            _notificationMessageRepository.UpdateNotification(notification);
            await _notificationMessageRepository.CompleteAsync();

            return NoContent();
        }

        // PATCH: api/notificationmessage/{id}/mark-as-sent
        [HttpPatch("{id}/mark-as-sent")]
        public async Task<IActionResult> MarkAsSent(int id)
        {
            var notification = await _notificationMessageRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            notification.SentAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            _notificationMessageRepository.UpdateNotification(notification);
            await _notificationMessageRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/notificationmessage/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _notificationMessageRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            _notificationMessageRepository.UpdateNotification(notification);
            await _notificationMessageRepository.CompleteAsync();

            return NoContent();
        }
    }
}
