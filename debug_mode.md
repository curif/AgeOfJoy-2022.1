# Age of Joy: Diagnostic & Bug Reporting Instructions

How to use the built-in diagnostic utility to report technical issues.

1. Activating Debug Mode
To capture detailed technical information, you must enable active logging before reproducing a bug:
*   Locate the **Configuration Cabinet** in your current room.
*   Switch to **Global Configuration Mode**.
*   Navigate to the **"Debug & Bug Report"** in the Global menu.
*   Toggle **Debug Logging** to **ON**. 

Once active, the system will record all internal engine events, script executions, and stack traces to a local diagnostic file.

2. Generating a Bug Report
After the issue has occurred (or if you have just experienced a hard crash), follow these steps:
*   Return to the **Debug & Bug Report** menu.
*   Select **"Generate Bug Report ZIP"**. 
*   Wait for the status message: *"ZIP created in AgeOfJoy dir!"*

3. Handling System Crashes (GPFs)
If the game crashes entirely to the Quest Home:
*   **IMPORTANT:** Re-launch the game but **DO NOT** toggle Debug Logging back on immediately. Toggling it ON creates a 
	new blank log, which will overwrite the log containing the crash data.
*   Go directly to the Debug menu and click **"Generate Bug Report ZIP"**. 
*   The system uses "Auto-Flush" technology, meaning the details of the crash are preserved on 
	your storage even if the application terminates abruptly.

4. Submitting Your Report
The utility will generate a file named `AgeOfJoy_BugReport_YYYYMMDD_HHMMSS.zip` located in your root `AgeOfJoy` data folder. This package contains:
*   The detailed debug log.
*   Your current hardware and system specifications.
*   A directory map of your installation.
*   Your current configuration (`.yaml`) files.

Please upload this ZIP file to the #bug-report channel along with a brief description of the actions leading up to the issue.
Yo can delete it if you want.
