<%@ Page Language="C#" %>

<!doctype html>
<html>
<head>
    <title>Reverse Proxy CDN</title>
    <meta charset="utf-8" />
    <style>
        body {
            font: 1em/1 arial;
            max-width: 960px;
            margin: 1em auto;
        }

        p {
            font-weight: bold;
        }

            p span {
                font-weight: normal;
            }
    </style>
</head>
<body>

    <h1>Reverse Proxy CDN</h1>
    
    <p>Location: <span><%= ConfigurationManager.AppSettings.Get("location") %></span></p>
</body>
</html>
