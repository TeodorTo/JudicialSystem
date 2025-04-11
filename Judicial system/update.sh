#!/bin/bash
cd /var/www/JudicialSystem/Judicial\ system/

# Спри сервиза
sudo systemctl stop judicialsystem.service

# Обнови от git
sudo git fetch origin
sudo git reset --hard origin/master

# Изтрий старата публикация
sudo rm -rf /var/www/JudicialSystem/Judicial\ system/publish/

# Публикувай проекта
sudo /usr/bin/dotnet publish -c Release -o /var/www/JudicialSystem/Judicial\ system/publish/

# Смени правата
sudo chown -R www-data:www-data /var/www/JudicialSystem/Judicial\ system/publish/

# Стартирай отново сервиза
sudo systemctl start judicialsystem.service
