namespace MorningDashboard

module CurrentTime =
    let getCurrentTime () : System.DateTimeOffset option =
        Some System.DateTimeOffset.Now