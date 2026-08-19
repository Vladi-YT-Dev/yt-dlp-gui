# yt-dlp-gui

A lightweight Graphical User Interface (GUI) wrapper for [yt-dlp](https://github.com/yt-dlp/yt-dlp).

---

## 🚀 Setup & Installation

1. **Download:** Grab the latest `.exe` from the [Releases](../../releases) page.
2. **Move File:** Place the downloaded `.exe` directly into your main `yt-dlp` directory (where `yt-dlp.exe` is located).
3. **Update yt-dlp:** 
   * Open Command Prompt (`cmd`) in that folder.
   * Run the following command to ensure yt-dlp is updated:
     ```cmd
     yt-dlp --update-to nightly
     ```
   * Close the command prompt once finished.

---

## 📖 How to Use

1. Launch the downloaded GUI application.
2. Copy your desired YouTube video URL to the clipboard.
3. Paste the URL into the **Link** text box in the application.

The GUI automatically triggers `yt-dlp` using the best MP4 quality format:
```bash
yt-dlp -f "bv*[ext=mp4]+ba[ext=m4a]/b[ext=mp4]" "<LINK>"
```

📁 **Output:** Downloaded files are saved automatically in your `yt-dlp` root folder.

---

## 🙏 Credits

* [yt-dlp](https://github.com/yt-dlp/yt-dlp)
* [FFmpeg](https://ffmpeg.org/)
* [Deno](https://deno.com/)
