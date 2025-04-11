#!/bin/bash
cd /var/www/JudicialSystem/Judicial\ system/
sudo git fetch origin
sudo git reset --hard origin/master
sudo /usr/bin/dotnet publish -c Release -o /var/www/JudicialSystem/Judicial\ system/publish/
sudo chown -R www-data:www-data /var/www/JudicialSystem/Judicial\ system/publish/
sudo systemctl restart judicialsystem.service