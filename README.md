# Tubes1_Cisa
> Proyek ini ditujukan untuk menyelesaikan tugas besar mata kuliah Strategi Algoritma IF2211. Di dalam direktori ini, tersedia 4 jenis bot yang bisa digunakan dalam permainan Robocode. 

# General Information
Repositori ini berisikan program untuk memenuhi penilaian mata kuliah Strategi Algoritma IF2211. Setiap program merupakan algoritma bagi robot dalam permainan Robocode. Terdapat 4 program dengan algoritma _greedy_ dengan fokus yang berbeda. Bahasa pemrograman yang digunakan dalam kode program adalah C#.

# Daftar Bot yang Dibuat
### 1. Bot Maniac (Greedy by Bullet Damage - Ram Damage - Ram Damage Bonus)
- Menghasilkan skor dengan tembakan dan tabrakan.
- Memanfaatkan bullet damage, ram damage, dan bonus tambahan dari tabrakan yang menghabisi lawan.

### 2. Bot Eits (Greedy by Bullet Damage - Bullet Damage Bonus - Survival Score)
- Mengoptimalkan bullet damage, bonus tembakan, dan survival score.
- Mengutamakan akurasi serangan dan menghindari tembakan musuh dengan strategi dodge.

### 3. Bot Lawrie (Greedy by Bullet Damage - Bullet Damage Bonus - Survival Score - Last Survival Score)
- Fokus pada bertahan hidup hingga akhir ronde.
- Menghindari kerumunan, memilih posisi aman, dan menyerang lawan secara strategis.

Setiap bot menggunakan pendekatan greedy untuk memaksimalkan skor dengan strategi uniknya masing-masing.

# Package Information
- Folder _src_ berisikan 4 folder lain yaitu folder _main-bot_ dan _alternative-bots_
- Folder _main-bot_ berisikan kode program untuk robot utama yang ingin digunakan
- Folder _alternative-bots_ berisikan 4 folder kode program robot lainnya.
- Folder _doc_ berisikan laporan dari tugas besar ini

# Getting Started
Sebelum dapat menggunakan kode program yang disediakan, Anda harus mengunduh _starter pack_ terlebih dahulu melalui tautan _https://www.google.com/url?q=https://github.com/Ariel-HS/tubes1-if2211-starter-pack/releases/tag/v1.0&sa=D&source=docs&ust=1742637664449880&usg=AOvVaw3bKBajhcYUSczuiVn1gILR_. Setelah itu, Anda bisa melakukan _setup_ melalui panduan yang bisa diakses melalui tautan _https://docs.google.com/document/d/12upAKLU9E7tS6-xMUpJZ8gA1L76YngZNCc70AaFgyMY/edit?usp=_

# Initial Configuration
Berikut langkah-langkah untuk menjalankan program.
1. Pastikan directory berada di file path “tubes1-if2211-starter-pack-1.0\tubes1-if2211-starter-pack-1.0” lalu jalankan GUI dengan mengetik “java -jar robocode-tankroyale-gui-0.30.0.jar” di terminal.
2. Lakukan set up config dengan cara klik tombol “Config -> Bot Root Directories”. Kemudian masukkan folder yang berisi sample bots atau bot yang telah dibuat.
3. Jalankan battle dengan cara klik tombol “Battle -> Start Battle”  akan muncul panel konfigurasi permainan. Boot bot yang ingin dimainkan dengan cara dan tambahkan bot ke dalam arena dengan cara klik tombol “Add”.
4. Mulai permainan dengan cara klik tombol “Start Battle”.

# Features 
- Robot pertama (main) merupakan implementasi algoritma _greedy_ berbasis _Bullet Damage - Ram Damage - Ram Damage Bonus_
- Robot kedua merupakan implementasi algoritma _greedy_ berbasis _Bullet Damage - Bullet Damage Bonus - Survival Score_
- Robot ketiga merupakan implementasi algoritma _greedy_ berbasis _Bullet Damage - Bullet Damage Bonus - Survival Score - Last Survival Score_
- Robot keempat merupakan implementasi algoritma _greedy_ berbasis

# Links
- Repository : https://github.com/varel183/Tubes1_Cisa
- Issue Tracker :
  Jika menemukan sebuah bug, evaluasi atau improvisasi pada robot ini dapat mengontak 13523008@std.stei.itb.ac.id / 13523029@std.stei.itb.ac.id / 13523046@std.stei.itb.ac.id

# Contributor
Oleh Kelompok Cisa
- Varel Tiara       13523008 (https://github.com/varel183)
- Bryan Ho          13523029 (https://github.com/bry-ho)
- Ivan Wirawan      13523046 (https://github.com/ivan-wirawan)
