## Possible Errors When Building the C# Project

## Mark of the Web Error in Cloned Files

After cloning this repository, you may encounter an error when building the project related to certain files, such as `Form1.resx`, being in the Internet or Restricted zone. This issue occurs because files downloaded from the internet, including those cloned from GitHub, may be flagged with a security marker known as the "Mark of the Web" (MOTW). 

This security feature is designed to protect your system but can prevent the project from building correctly.

## Steps to Resolve Form1.resx Security Issue

If you encounter issues with the `Form1.resx` file being marked as from the "Internet or Restricted zone," follow these steps to resolve the problem:

1. **Locate the `Form1.resx` File:**
   - Open File Explorer and navigate to the directory where your project is located.
   - Find the `Form1.resx` file. It should be in the same directory as your form files (`Form1.cs`, `Form1.Designer.cs`).

2. **Check File Properties:**
   - Right-click on the `Form1.resx` file and select **Properties** from the context menu.
   - In the **General** tab, look for a section labeled **Security** at the bottom of the window.
   - If you see a checkbox labeled **Unblock** (with a message indicating that the file came from another computer), check the box to unblock the file.
   - Click **Apply** and then **OK** to save your changes.

3. **Open the Project:**
   - Open Visual Studio and load your project.
   - Attempt to run the project.

4. **If an Error Occurs:**
   - If you still encounter errors related to the `Form1.resx` file, close Visual Studio completely.
   - Reopen Visual Studio and load your project again.
   - Rerun the project to see if the issue is resolved.

These steps should help you resolve any security-related issues with the `Form1.resx` file and allow your project to build and run correctly.

## Designer Form Not Displayed

**Issue:** The designer form may not be displayed correctly in Visual Studio after building the project.

**Reason:** This issue may occur due to temporary glitches or caching problems within Visual Studio.

**Solution:** A simple reboot of Visual Studio or your computer typically resolves this issue. After rebooting, the designer form should display correctly.


