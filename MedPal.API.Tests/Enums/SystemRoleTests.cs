using System;
using MedPal.API.Enums;

namespace MedPal.API.Tests.Enums
{
    /// <summary>
    /// Fase 1: SystemRole debe incluir Nurse con el valor 5 y mantener la jerarquía
    /// 1=SuperAdmin, 2=AccountAdmin, 3=ClinicAdmin, 4=HealthProfessional, 5=Nurse,
    /// 6=Receptionist, 7=Patient.
    /// </summary>
    public class SystemRoleTests
    {
        [Fact]
        public void SystemRole_Nurse_IsDefinedWithValue5()
        {
            Assert.True(Enum.IsDefined(typeof(SystemRole), 5));
            Assert.Equal(5, (int)SystemRole.Nurse);
            Assert.Equal("Nurse", SystemRole.Nurse.ToString());
        }

        [Fact]
        public void SystemRole_HasExpectedOrderingWithoutGaps()
        {
            Assert.Equal(1, (int)SystemRole.SuperAdmin);
            Assert.Equal(2, (int)SystemRole.AccountAdmin);
            Assert.Equal(3, (int)SystemRole.ClinicAdmin);
            Assert.Equal(4, (int)SystemRole.HealthProfessional);
            Assert.Equal(5, (int)SystemRole.Nurse);
            Assert.Equal(6, (int)SystemRole.Receptionist);
            Assert.Equal(7, (int)SystemRole.Patient);
        }
    }
}