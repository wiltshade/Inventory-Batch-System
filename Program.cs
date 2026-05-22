using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EventRegistrationSystem
{
    class Program
    {
        static string dataFolder = "data";
        static string eventFile = Path.Combine(dataFolder, "registrations.txt");
        static string auditFile = Path.Combine(dataFolder, "audit_log.txt");

        static void Main(string[] args)
        {
            InitializeStorage();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=======================================");
                Console.WriteLine("     EVENT REGISTRATION SYSTEM");
                Console.WriteLine("=======================================");
                Console.WriteLine("1. Add Registration");
                Console.WriteLine("2. View Registrations");
                Console.WriteLine("3. Search Registration");
                Console.WriteLine("4. Update Registration");
                Console.WriteLine("5. Soft Delete Registration");
                Console.WriteLine("6. Hard Delete Registration");
                Console.WriteLine("7. Generate Event Report");
                Console.WriteLine("8. Exit");
                Console.WriteLine("=======================================");
                Console.Write("Choose option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddRegistration();
                        break;

                    case "2":
                        ViewRegistrations();
                        break;

                    case "3":
                        SearchRegistration();
                        break;

                    case "4":
                        UpdateRegistration();
                        break;

                    case "5":
                        SoftDeleteRegistration();
                        break;

                    case "6":
                        HardDeleteRegistration();
                        break;

                    case "7":
                        GenerateReport();
                        break;

                    case "8":
                        Console.WriteLine("Exiting...");
                        return;

                    default:
                        Console.WriteLine("Invalid choice.");
                        Pause();
                        break;
                }
            }
        }

        // =====================================
        // INITIALIZE STORAGE
        // =====================================
        static void InitializeStorage()
        {
            try
            {
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                if (!File.Exists(eventFile))
                {
                    File.Create(eventFile).Close();
                }

                if (!File.Exists(auditFile))
                {
                    File.Create(auditFile).Close();
                }

                LogAudit("SYSTEM", "Storage initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Initialization Error: " + ex.Message);
            }
        }

        // =====================================
        // ADD REGISTRATION
        // =====================================
        static void AddRegistration()
        {
            Console.Clear();
            Console.WriteLine("===== ADD REGISTRATION =====");

            try
            {
                int recordId = GenerateRecordId();

                Console.Write("Participant Name: ");
                string participantName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(participantName))
                {
                    Console.WriteLine("Participant name is required.");
                    Pause();
                    return;
                }

                Console.Write("Event Name: ");
                string eventName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(eventName))
                {
                    Console.WriteLine("Event name is required.");
                    Pause();
                    return;
                }

                Console.Write("Email Address: ");
                string email = Console.ReadLine();

                if (!email.Contains("@"))
                {
                    Console.WriteLine("Invalid email address.");
                    Pause();
                    return;
                }

                Console.Write("Number of Guests: ");
                int guests;

                if (!int.TryParse(Console.ReadLine(), out guests) || guests < 0)
                {
                    Console.WriteLine("Invalid guest count.");
                    Pause();
                    return;
                }

                string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string updatedAt = createdAt;
                bool isActive = true;

                string rawData =
                    recordId +
                    participantName +
                    eventName +
                    email +
                    guests;

                string checksum = GenerateChecksum(rawData);

                string record =
                    recordId + "|" +
                    participantName + "|" +
                    eventName + "|" +
                    email + "|" +
                    guests + "|" +
                    createdAt + "|" +
                    updatedAt + "|" +
                    isActive + "|" +
                    checksum;

                File.AppendAllText(eventFile, record + Environment.NewLine);

                Console.WriteLine("Registration added successfully.");

                LogAudit("ADD", "Registration ID " + recordId + " added.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // VIEW REGISTRATIONS
        // =====================================
        static void ViewRegistrations()
        {
            Console.Clear();
            Console.WriteLine("===== ACTIVE REGISTRATIONS =====");

            try
            {
                string[] lines = File.ReadAllLines(eventFile);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('|');

                    if (parts.Length < 9)
                    {
                        LogAudit("ERROR", "Malformed record found.");
                        continue;
                    }

                    bool isActive = Convert.ToBoolean(parts[7]);

                    if (isActive)
                    {
                        Console.WriteLine("-----------------------------------");
                        Console.WriteLine("Record ID       : " + parts[0]);
                        Console.WriteLine("Participant     : " + parts[1]);
                        Console.WriteLine("Event Name      : " + parts[2]);
                        Console.WriteLine("Email           : " + parts[3]);
                        Console.WriteLine("Guests          : " + parts[4]);
                        Console.WriteLine("Created At      : " + parts[5]);
                        Console.WriteLine("Updated At      : " + parts[6]);
                    }
                }

                LogAudit("READ", "Viewed registrations.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // SEARCH REGISTRATION
        // =====================================
        static void SearchRegistration()
        {
            Console.Clear();
            Console.WriteLine("===== SEARCH REGISTRATION =====");

            Console.Write("Enter participant or event name: ");
            string keyword = Console.ReadLine().ToLower();

            try
            {
                string[] lines = File.ReadAllLines(eventFile);

                bool found = false;

                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');

                    if (parts.Length < 9)
                        continue;

                    bool isActive = Convert.ToBoolean(parts[7]);

                    if (isActive &&
                        (parts[1].ToLower().Contains(keyword) ||
                         parts[2].ToLower().Contains(keyword)))
                    {
                        found = true;

                        Console.WriteLine("-----------------------------------");
                        Console.WriteLine("Record ID   : " + parts[0]);
                        Console.WriteLine("Participant : " + parts[1]);
                        Console.WriteLine("Event Name  : " + parts[2]);
                        Console.WriteLine("Email       : " + parts[3]);
                    }
                }

                if (!found)
                {
                    Console.WriteLine("No matching registration found.");
                }

                LogAudit("READ", "Searched registrations.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // UPDATE REGISTRATION
        // =====================================
        static void UpdateRegistration()
        {
            Console.Clear();
            Console.WriteLine("===== UPDATE REGISTRATION =====");

            Console.Write("Enter Record ID: ");
            string id = Console.ReadLine();

            try
            {
                List<string> lines = File.ReadAllLines(eventFile).ToList();

                bool found = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string[] parts = lines[i].Split('|');

                    if (parts[0] == id)
                    {
                        found = true;

                        Console.Write("New Participant Name: ");
                        string participantName = Console.ReadLine();

                        Console.Write("New Event Name: ");
                        string eventName = Console.ReadLine();

                        Console.Write("New Email Address: ");
                        string email = Console.ReadLine();

                        Console.Write("New Number of Guests: ");
                        int guests;

                        if (!int.TryParse(Console.ReadLine(), out guests))
                        {
                            Console.WriteLine("Invalid guest count.");
                            Pause();
                            return;
                        }

                        string updatedAt =
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string rawData =
                            id +
                            participantName +
                            eventName +
                            email +
                            guests;

                        string checksum = GenerateChecksum(rawData);

                        lines[i] =
                            id + "|" +
                            participantName + "|" +
                            eventName + "|" +
                            email + "|" +
                            guests + "|" +
                            parts[5] + "|" +
                            updatedAt + "|" +
                            parts[7] + "|" +
                            checksum;

                        break;
                    }
                }

                if (found)
                {
                    File.WriteAllLines(eventFile, lines);

                    Console.WriteLine("Registration updated successfully.");

                    LogAudit("UPDATE", "Registration ID " + id + " updated.");
                }
                else
                {
                    Console.WriteLine("Record not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // SOFT DELETE
        // =====================================
        static void SoftDeleteRegistration()
        {
            Console.Clear();
            Console.WriteLine("===== SOFT DELETE =====");

            Console.Write("Enter Record ID: ");
            string id = Console.ReadLine();

            try
            {
                List<string> lines = File.ReadAllLines(eventFile).ToList();

                bool found = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string[] parts = lines[i].Split('|');

                    if (parts[0] == id)
                    {
                        found = true;

                        parts[7] = "False";

                        lines[i] = string.Join("|", parts);

                        break;
                    }
                }

                if (found)
                {
                    File.WriteAllLines(eventFile, lines);

                    Console.WriteLine("Registration soft deleted.");

                    LogAudit("DELETE", "Soft delete ID " + id);
                }
                else
                {
                    Console.WriteLine("Record not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // HARD DELETE
        // =====================================
        static void HardDeleteRegistration()
        {
            Console.Clear();
            Console.WriteLine("===== HARD DELETE =====");

            Console.Write("Enter Record ID: ");
            string id = Console.ReadLine();

            try
            {
                List<string> lines = File.ReadAllLines(eventFile).ToList();

                int removed =
                    lines.RemoveAll(line => line.StartsWith(id + "|"));

                File.WriteAllLines(eventFile, lines);

                if (removed > 0)
                {
                    Console.WriteLine("Registration permanently deleted.");

                    LogAudit("DELETE", "Hard delete ID " + id);
                }
                else
                {
                    Console.WriteLine("Record not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // GENERATE REPORT
        // =====================================
        static void GenerateReport()
        {
            Console.Clear();
            Console.WriteLine("===== EVENT REPORT =====");

            try
            {
                string[] lines = File.ReadAllLines(eventFile);

                int totalParticipants = 0;
                int totalGuests = 0;

                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');

                    if (parts.Length < 9)
                        continue;

                    bool isActive = Convert.ToBoolean(parts[7]);

                    if (isActive)
                    {
                        totalParticipants++;
                        totalGuests += Convert.ToInt32(parts[4]);
                    }
                }

                Console.WriteLine("Total Active Participants : " +
                                  totalParticipants);

                Console.WriteLine("Total Guests Registered   : " +
                                  totalGuests);

                LogAudit("REPORT", "Generated event report.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                LogAudit("ERROR", ex.Message);
            }

            Pause();
        }

        // =====================================
        // GENERATE RECORD ID
        // =====================================
        static int GenerateRecordId()
        {
            try
            {
                string[] lines = File.ReadAllLines(eventFile);

                if (lines.Length == 0)
                    return 1;

                int lastId = 0;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('|');

                    int currentId;

                    if (int.TryParse(parts[0], out currentId))
                    {
                        if (currentId > lastId)
                        {
                            lastId = currentId;
                        }
                    }
                }

                return lastId + 1;
            }
            catch
            {
                return 1;
            }
        }

        // =====================================
        // CHECKSUM
        // =====================================
        static string GenerateChecksum(string data)
        {
            int sum = 0;

            foreach (char c in data)
            {
                sum += c;
            }

            return sum.ToString();
        }

        // =====================================
        // AUDIT LOGGER
        // =====================================
        static void LogAudit(string action, string details)
        {
            try
            {
                string log =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " | "
                    + action
                    + " | "
                    + details;

                File.AppendAllText(
                    auditFile,
                    log + Environment.NewLine
                );
            }
            catch
            {
            }
        }

        // =====================================
        // PAUSE
        // =====================================
        static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}