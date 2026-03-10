// นำเข้า namespace System ซึ่งเป็น namespace พื้นฐานของ .NET มี class หลักๆ เช่น Object, String, Console, Exception
using System;
// นำเข้า System.Collections.Generic สำหรับ collection แบบ generic เช่น List<T>, Dictionary<K,V>
// แม้ไฟล์นี้ไม่ได้ใช้โดยตรง แต่เป็น template มาตรฐานของ Visual Studio ที่ใส่ไว้ให้
using System.Collections.Generic;
// นำเข้า System.Linq สำหรับ LINQ (Language Integrated Query) ที่ใช้ query ข้อมูลแบบ functional
// เช่น .Where(), .Select(), .FirstOrDefault() — เป็น template มาตรฐานที่ VS ใส่ไว้ให้
using System.Linq;
// นำเข้า System.Threading.Tasks สำหรับ asynchronous programming เช่น Task, async/await
// เป็น template มาตรฐานที่ VS ใส่ไว้ให้ แม้โปรแกรมนี้ไม่ได้ใช้ async
using System.Threading.Tasks;
// นำเข้า System.Windows.Forms ซึ่งเป็น namespace หลักของ Windows Forms GUI framework
// มี class สำคัญ เช่น Application (จัดการ lifecycle ของแอป), Form (หน้าต่าง), Button, TextBox ฯลฯ
using System.Windows.Forms;

// ประกาศ namespace "Steganography" ให้ตรงกับ namespace ของ class อื่นๆ ในโปรเจค
// เพื่อให้ class ภายใน namespace เดียวกันเรียกใช้กันได้โดยไม่ต้อง using เพิ่ม
namespace Steganography
{
    // ประกาศ class Program เป็น internal (เข้าถึงได้เฉพาะภายใน assembly/โปรเจคนี้เท่านั้น)
    // และ static (ไม่ต้องสร้าง instance, ไม่สามารถสร้าง new Program() ได้)
    // class นี้มีหน้าที่เดียวคือเป็นจุดเริ่มต้น (entry point) ของโปรแกรม
    internal static class Program
    {
        /// <summary>
        /// จุดเริ่มต้นหลัก (Main Entry Point) ของแอปพลิเคชัน
        /// .NET Runtime จะเรียกเมธอด Main() เป็นเมธอดแรกเมื่อโปรแกรมเริ่มทำงาน
        /// </summary>
        //
        // [STAThread] = Single-Threaded Apartment
        // เป็น attribute ที่กำหนดว่า thread หลักของโปรแกรมใช้ COM threading model แบบ STA
        // จำเป็นสำหรับ Windows Forms เพราะ:
        //   1) UI controls ของ Windows ทำงานบน STA thread เท่านั้น
        //   2) ฟีเจอร์ต่างๆ เช่น Clipboard (คัดลอก/วาง), Drag & Drop, OpenFileDialog, SaveFileDialog
        //      ใช้ COM objects ที่ต้องการ STA threading model
        //   3) ถ้าไม่ใส่ [STAThread] → ฟีเจอร์เหล่านี้อาจทำงานผิดพลาดหรือ crash
        [STAThread]
        static void Main()
        {
            // เปิดใช้ Visual Styles ของ Windows (เช่น ปุ่มแบบ Aero/Modern แทนที่จะเป็นแบบ Classic)
            // ทำให้ UI ดูทันสมัยตามธีมของ Windows ที่ผู้ใช้เลือก (Windows 7, 10, 11)
            // ถ้าไม่เรียก → controls จะแสดงผลแบบ Windows Classic (สี่เหลี่ยมเรียบๆ ไม่มีเอฟเฟกต์)
            Application.EnableVisualStyles();

            // ตั้งค่าให้ controls ใหม่ใช้ GDI+ (false) แทน GDI สำหรับการ render ข้อความ
            // false = ใช้ GDI+ ซึ่งเป็นค่าเริ่มต้นของ .NET 2.0+ และให้ผลลัพธ์ที่สอดคล้องกัน
            // ต้องเรียกก่อนสร้าง control ใดๆ (ก่อน new SteganographyForm())
            // ถ้าเรียกหลังสร้าง control จะ throw InvalidOperationException
            Application.SetCompatibleTextRenderingDefault(false);

            // สร้าง instance ของ SteganographyForm (หน้าต่างหลักของแอป) แล้วเริ่ม message loop
            // Application.Run() ทำสิ่งต่อไปนี้:
            //   1) แสดงฟอร์มที่ส่งเข้ามา (SteganographyForm) บนหน้าจอ
            //   2) เริ่ม Windows Message Loop — วนรับ message จาก OS (เช่น คลิกเมาส์, กดปุ่ม, ปรับขนาดหน้าต่าง)
            //      แล้วส่งต่อไปยัง event handler ที่เหมาะสม
            //   3) เมื่อผู้ใช้ปิดฟอร์มหลัก (กดปุ่ม X) → message loop จะหยุด → Application.Run() return
            //   4) โปรแกรมจะจบการทำงาน (Main() return → process exit)
            Application.Run(new SteganographyForm());
        }
    }
}
