# Stage 1: Build - ใช้ SDK 10.0 ในการ Compile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# คัดลอกไฟล์ .csproj เพื่อ Restore dependencies (ช่วยให้ Build เร็วขึ้นในครั้งถัดไป)
COPY ["testforproject.csproj", "./"]
RUN dotnet restore "testforproject.csproj"

# คัดลอกไฟล์ทั้งหมดและสั่ง Publish
COPY . .
RUN dotnet publish "testforproject.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime - ใช้ ASP.NET 10.0 (ตัวเล็กและเบากว่า)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# ตั้งค่า Port (ปกติ .NET 8 ขึ้นไปจะใช้ Port 8080 เป็นค่าเริ่มต้น)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "testforproject.dll"]