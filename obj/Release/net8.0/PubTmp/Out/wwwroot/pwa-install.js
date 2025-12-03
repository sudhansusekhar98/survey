// PWA Installation Handler
let deferredPrompt;
const installButton = document.getElementById('pwa-install-btn');
const installBanner = document.getElementById('pwa-install-banner');
const installMenuItem = document.getElementById('pwa-install-menu-item');
const installMenuLink = document.getElementById('pwa-install-menu-link');

// Debug: Log when script loads
console.log('PWA Install script loaded');
console.log('Install button found:', !!installButton);
console.log('Install banner found:', !!installBanner);
console.log('Install menu item found:', !!installMenuItem);

// Check if already installed
const isInstalled = window.matchMedia('(display-mode: standalone)').matches || 
                    window.navigator.standalone === true;

if (isInstalled) {
  console.log('App is already running in standalone mode - hiding install prompts');
} else {
  console.log('App is running in browser mode - install available');
}

// Listen for the beforeinstallprompt event
window.addEventListener('beforeinstallprompt', (e) => {
  console.log('✅ beforeinstallprompt event fired - PWA is installable!');
  // Prevent the mini-infobar from appearing on mobile
  e.preventDefault();
  // Stash the event so it can be triggered later
  deferredPrompt = e;
  
  // Show the install button/banner/menu
  if (installButton && !isInstalled) {
    installButton.style.display = 'inline-block';
    console.log('Install button shown');
  }
  if (installBanner && !isInstalled) {
    installBanner.style.display = 'block';
    console.log('Install banner shown');
  }
  if (installMenuItem && !isInstalled) {
    installMenuItem.style.display = 'block';
    console.log('Install menu item shown');
  }
});

// Function to trigger install
async function triggerInstall() {
  console.log('Install triggered');
  console.log('Deferred prompt available:', !!deferredPrompt);
  
  if (!deferredPrompt) {
    console.warn('No deferred prompt available.');
    
    // Check if already installed
    if (isInstalled) {
      alert('The app is already installed!');
    } else {
      // Provide helpful message
      alert('To install this app:\n\n' +
            '1. Make sure you are using Chrome, Edge, or Safari\n' +
            '2. Visit the site via HTTPS\n' +
            '3. Look for install icon in address bar\n' +
            '4. On mobile: Use "Add to Home Screen" from browser menu\n\n' +
            'Note: You may need to interact with the site first before installation is offered.');
    }
    return;
  }
  
  // Show the install prompt
  deferredPrompt.prompt();
  
  // Wait for the user to respond to the prompt
  const { outcome } = await deferredPrompt.userChoice;
  console.log(`User response to the install prompt: ${outcome}`);
  
  if (outcome === 'accepted') {
    console.log('User accepted the install prompt');
  } else {
    console.log('User dismissed the install prompt');
  }
  
  // Clear the deferred prompt variable
  deferredPrompt = null;
  
  // Hide the install UI
  if (installButton) installButton.style.display = 'none';
  if (installBanner) installBanner.style.display = 'none';
  if (installMenuItem) installMenuItem.style.display = 'none';
}

// Install button click handler
if (installButton) {
  installButton.addEventListener('click', triggerInstall);
} else {
  console.warn('Install button element not found in DOM');
}

// Install menu link click handler
if (installMenuLink) {
  installMenuLink.addEventListener('click', triggerInstall);
}

// Listen for the app installed event
window.addEventListener('appinstalled', () => {
  console.log('PWA was installed');
  // Hide the install button
  if (installButton) {
    installButton.style.display = 'none';
  }
  if (installBanner) {
    installBanner.style.display = 'none';
  }
  deferredPrompt = null;
});

// Check if app is already installed
if (window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true) {
  console.log('App is running in standalone mode');
  // Hide install prompts
  if (installButton) {
    installButton.style.display = 'none';
  }
  if (installBanner) {
    installBanner.style.display = 'none';
  }
}
