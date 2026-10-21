using System;
using System.Collections.Generic;
using MedPal.API.Models;
using MedPal.API.Utils;
using Xunit;

namespace MedPal.API.Tests.Utils
{
    public class SlotAvailabilityTests
    {
        // ---------- IntervalsOverlap ----------

        [Theory]
        [InlineData(9, 0, 10, 0, 9, 30, 10, 30)]   // bloque dentro de cita
        [InlineData(9, 30, 10, 30, 9, 0, 10, 0)]   // bloque cruza el inicio de cita
        [InlineData(9, 0, 9, 30, 9, 0, 10, 0)]      // inicio exacto
        [InlineData(9, 0, 12, 0, 10, 0, 11, 0)]     // cita dentro de bloque grande
        public void IntervalsOverlap_OverlappingIntervals_ReturnsTrue(
            int s1h, int s1m, int e1h, int e1m,
            int s2h, int s2m, int e2h, int e2m)
        {
            var s1 = new TimeOnly(s1h, s1m);
            var e1 = new TimeOnly(e1h, e1m);
            var s2 = new TimeOnly(s2h, s2m);
            var e2 = new TimeOnly(e2h, e2m);

            Assert.True(SlotAvailability.IntervalsOverlap(s1, e1, s2, e2));
            Assert.True(SlotAvailability.IntervalsOverlap(s2, e2, s1, e1)); // simétrico
        }

        [Theory]
        [InlineData(9, 0, 9, 30, 9, 30, 10, 0)]   // fin == inicio
        [InlineData(9, 0, 9, 30, 10, 0, 11, 0)]   // disjuntos (hueco entre ambos)
        [InlineData(10, 0, 11, 0, 9, 0, 9, 30)]   // disjuntos (bloque antes de cita)
        public void IntervalsOverlap_NonOverlappingIntervals_ReturnsFalse(
            int s1h, int s1m, int e1h, int e1m,
            int s2h, int s2m, int e2h, int e2m)
        {
            var s1 = new TimeOnly(s1h, s1m);
            var e1 = new TimeOnly(e1h, e1m);
            var s2 = new TimeOnly(s2h, s2m);
            var e2 = new TimeOnly(e2h, e2m);

            Assert.False(SlotAvailability.IntervalsOverlap(s1, e1, s2, e2));
            Assert.False(SlotAvailability.IntervalsOverlap(s2, e2, s1, e1));
        }

        // ---------- IsSlotBlocked (bloque vs slot) ----------

        [Fact]
        public void IsSlotBlocked_SlotInsideBlock_ReturnsTrue()
        {
            var blocks = new List<AvailabilityBlock>
            {
                new AvailabilityBlock { StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0) }
            };

            Assert.True(SlotAvailability.IsSlotBlocked(new TimeOnly(10, 0), new TimeOnly(10, 30), blocks));
        }

        [Fact]
        public void IsSlotBlocked_SlotTouchesBlockStart_NotBlocked()
        {
            var blocks = new List<AvailabilityBlock>
            {
                new AvailabilityBlock { StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(11, 0) }
            };

            // Slot [9:00, 9:30) termina justo cuando empieza el bloqueo: no solapa.
            Assert.False(SlotAvailability.IsSlotBlocked(new TimeOnly(9, 0), new TimeOnly(9, 30), blocks));
        }

        [Fact]
        public void IsSlotBlocked_SlotOverlapsClinicWideBlock_ReturnsTrue()
        {
            var blocks = new List<AvailabilityBlock>
            {
                new AvailabilityBlock { DoctorId = null, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(18, 0) }
            };

            var isBlocked = SlotAvailability.IsSlotBlocked(new TimeOnly(9, 0), new TimeOnly(9, 30), blocks);
            Assert.True(isBlocked);
        }

        [Fact]
        public void IsSlotBlocked_NoBlocks_ReturnsFalse()
        {
            Assert.False(SlotAvailability.IsSlotBlocked(new TimeOnly(9, 0), new TimeOnly(9, 30), new List<AvailabilityBlock>()));
        }

        // ---------- IsSlotBlockedByAppointment ----------

        [Fact]
        public void IsSlotBlockedByAppointment_AppointmentOverlapsSlot_ReturnsTrue()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Time = new TimeOnly(9, 0), DurationMinutes = 60 }
            };

            Assert.True(SlotAvailability.IsSlotBlockedByAppointment(new TimeOnly(9, 30), new TimeOnly(10, 0), appointments));
        }

        [Fact]
        public void IsSlotBlockedByAppointment_AppointmentEndsExactlyAtSlotStart_NotBlocked()
        {
            var appointments = new List<Appointment>
            {
                new Appointment { Time = new TimeOnly(8, 30), DurationMinutes = 30 }
            };

            // Cita [8:30, 9:00); slot [9:00, 9:30): contiguos, no solapan.
            Assert.False(SlotAvailability.IsSlotBlockedByAppointment(new TimeOnly(9, 0), new TimeOnly(9, 30), appointments));
        }
    }
}