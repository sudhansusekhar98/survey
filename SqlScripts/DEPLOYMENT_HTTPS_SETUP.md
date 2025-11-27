# HTTPS Setup for Geolocation API

## Issue
The Geolocation API requires a **secure context (HTTPS)** to function in production. If your application is deployed over HTTP, location fetching will fail with the error:
```
Unable to fetch location: Only secure origins are allowed
```

## Solutions

### Option 1: Enable HTTPS on IIS (Recommended for Production)

1. **Obtain an SSL Certificate:**
   - Purchase from a Certificate Authority (CA), or
   - Use a free certificate from [Let's Encrypt](https://letsencrypt.org/), or
   - Use a self-signed certificate for internal testing

2. **Install Certificate in IIS:**
   - Open IIS Manager
   - Select your server node
   - Double-click "Server Certificates"
   - Click "Import" (right panel) and select your .pfx certificate file
   - Enter the certificate password if required

3. **Bind HTTPS to Your Site:**
   - In IIS Manager, select your website
   - Click "Bindings" (right panel)
   - Click "Add"
   - Type: `https`
   - Port: `443`
   - SSL Certificate: Select your installed certificate
   - Click OK

4. **Enable HTTPS Redirection:**
   - The application already has `app.UseHttpsRedirection();` in `Program.cs`
   - This will automatically redirect HTTP requests to HTTPS

5. **Update Firewall:**
   - Ensure port 443 is open in your firewall
   - Allow inbound HTTPS traffic

### Option 2: Kestrel with HTTPS Certificate

If deploying directly with Kestrel (without IIS):

1. **Add certificate to `appsettings.json`:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "path/to/certificate.pfx",
          "Password": "your-password"
        }
      }
    }
  }
}
```

2. **Or use development certificate for testing:**
```bash
dotnet dev-certs https --trust
```

### Option 3: Reverse Proxy (Nginx/Apache)

Use a reverse proxy to handle SSL termination:

**Nginx Example:**
```nginx
server {
    listen 443 ssl;
    server_name your-domain.com;
    
    ssl_certificate /path/to/certificate.crt;
    ssl_certificate_key /path/to/private.key;
    
    location / {
        proxy_pass http://localhost:5016;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Option 4: Manual Coordinate Entry (Fallback)

The application now includes improved error handling that:
- Detects non-HTTPS connections
- Provides clear error messages
- Allows manual coordinate entry as a fallback

Users can:
1. Use Google Maps to find coordinates
2. Right-click on a location → "What's here?"
3. Copy latitude and longitude
4. Paste into the form fields

## Testing Locally

For local development, the application supports both HTTP and HTTPS:

- **HTTP:** http://localhost:5016 (geolocation will NOT work)
- **HTTPS:** https://localhost:7041 (geolocation WILL work)

Always use the HTTPS URL for testing location features.

## Verification

After enabling HTTPS:

1. Access your application via `https://your-domain.com`
2. Check browser address bar for the lock icon 🔒
3. Try the "Fetch Location" button
4. Browser should prompt for location permission
5. Coordinates should populate automatically

## Browser Permissions

Even with HTTPS enabled, users must grant location permission:
- Click the location icon in the browser address bar
- Select "Allow" for location access
- This permission is remembered for future visits

## Troubleshooting

**Certificate Errors:**
- Ensure certificate is valid and not expired
- Certificate must match your domain name
- Trust the certificate in Windows Certificate Store

**Mixed Content Warnings:**
- Ensure all resources (CSS, JS, images) use HTTPS URLs
- Check browser console for mixed content errors

**Geolocation Still Failing:**
- Clear browser cache and cookies
- Check browser location permissions
- Verify GPS is enabled on mobile devices
- Try in incognito/private mode to rule out extension conflicts

## Additional Resources

- [MDN: Geolocation API](https://developer.mozilla.org/en-US/docs/Web/API/Geolocation_API)
- [Let's Encrypt: Free SSL Certificates](https://letsencrypt.org/)
- [ASP.NET Core HTTPS Configuration](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
