namespace ClinicManagementSystem.Domain.Constants;

public static class AppPermissions
{
    // ── Auth: User Management ─────────────────────────────────────────────────
    public static class Auth
    {
        public static class User
        {
            public const string View = "Auth.User.View";
            public const string Create = "Auth.User.Create";
            public const string Update = "Auth.User.Update";
            public const string Delete = "Auth.User.Delete";
            public const string ResetPassword = "Auth.User.ResetPassword";
        }

        public static class Role
        {
            public const string View = "Auth.Role.View";
            public const string Manage = "Auth.Role.Manage";
        }
    }

    // ── Clinical: Patients ────────────────────────────────────────────────────
    public static class Clinical
    {
        public static class Patient
        {
            public const string View = "Clinical.Patient.View";
            public const string Create = "Clinical.Patient.Create";
            public const string Update = "Clinical.Patient.Update";
            public const string Delete = "Clinical.Patient.Delete";
        }

        public static class Appointment
        {
            public const string View = "Clinical.Appointment.View";
            public const string Create = "Clinical.Appointment.Create";
            public const string Update = "Clinical.Appointment.Update";
            public const string Cancel = "Clinical.Appointment.Cancel";
            public const string CheckIn = "Clinical.Appointment.CheckIn";
        }

        public static class MedRecord
        {
            public const string View = "Clinical.MedRecord.View";
            public const string Create = "Clinical.MedRecord.Create";
            public const string Update = "Clinical.MedRecord.Update";
        }

        public static class Prescription
        {
            public const string View = "Clinical.Prescription.View";
            public const string Create = "Clinical.Prescription.Create";
            public const string Void = "Clinical.Prescription.Void";
        }
    }

    // ── Lab ───────────────────────────────────────────────────────────────────
    public static class Lab
    {
        public static class Order
        {
            public const string Create = "Lab.Order.Create";
            public const string View = "Lab.Order.View";
        }

        public static class Result
        {
            public const string Enter = "Lab.Result.Enter";
            public const string View = "Lab.Result.View";
        }
    }

    // ── Billing ───────────────────────────────────────────────────────────────
    public static class Billing
    {
        public static class Invoice
        {
            public const string View = "Billing.Invoice.View";
            public const string Create = "Billing.Invoice.Create";
            public const string Update = "Billing.Invoice.Update";
        }

        public static class Payment
        {
            public const string Record = "Billing.Payment.Record";
            public const string Refund = "Billing.Payment.Refund";
        }

        public static class Report
        {
            public const string View = "Billing.Report.View";
        }
    }

    // ── Admin ─────────────────────────────────────────────────────────────────
    public static class Admin
    {
        public static class Doctor
        {
            public const string View = "Admin.Doctor.View";
            public const string Manage = "Admin.Doctor.Manage";
        }
    }

    // ── System ────────────────────────────────────────────────────────────────
    public static class System
    {
        public static class AuditLog
        {
            public const string View = "System.AuditLog.View";
        }

        public static class Config
        {
            public const string View = "System.Config.View";
            public const string Manage = "System.Config.Manage";
        }

        public static class Report
        {
            public const string Export = "System.Report.Export";
        }
    }

    /// <summary>Returns every permission code as a flat enumerable (useful for seeding).</summary>
    public static IEnumerable<string> All()
    {
        yield return Auth.User.View;
        yield return Auth.User.Create;
        yield return Auth.User.Update;
        yield return Auth.User.Delete;
        yield return Auth.User.ResetPassword;
        yield return Auth.Role.View;
        yield return Auth.Role.Manage;
        yield return Clinical.Patient.View;
        yield return Clinical.Patient.Create;
        yield return Clinical.Patient.Update;
        yield return Clinical.Patient.Delete;
        yield return Clinical.Appointment.View;
        yield return Clinical.Appointment.Create;
        yield return Clinical.Appointment.Update;
        yield return Clinical.Appointment.Cancel;
        yield return Clinical.Appointment.CheckIn;
        yield return Clinical.MedRecord.View;
        yield return Clinical.MedRecord.Create;
        yield return Clinical.MedRecord.Update;
        yield return Clinical.Prescription.View;
        yield return Clinical.Prescription.Create;
        yield return Clinical.Prescription.Void;
        yield return Lab.Order.Create;
        yield return Lab.Order.View;
        yield return Lab.Result.Enter;
        yield return Lab.Result.View;
        yield return Billing.Invoice.View;
        yield return Billing.Invoice.Create;
        yield return Billing.Invoice.Update;
        yield return Billing.Payment.Record;
        yield return Billing.Payment.Refund;
        yield return Billing.Report.View;
        yield return Admin.Doctor.View;
        yield return Admin.Doctor.Manage;
        yield return System.AuditLog.View;
        yield return System.Config.View;
        yield return System.Config.Manage;
        yield return System.Report.Export;
    }
}
