// PWA Installation Handler
let deferredPrompt;
const installButton = document.getElementById('pwa-install-btn');
const installBanner = document.getElementById('pwa-install-banner');

// Debug: Log when script loads
console.log('PWA Install script loaded');
console.log('Install button found:', !!installButton);
console.log('Install banner found:', !!installBanner);

// Listen for the beforeinstallprompt event
window.addEventListener('beforeinstallprompt', (e) => {
  console.log('beforeinstallprompt event fired');
  // Prevent the mini-infobar from appearing on mobile
  e.preventDefault();
  // Stash the event so it can be triggered later
  deferredPrompt = e;
  // Show the install button/banner
  if (installButton) {
    installButton.style.display = 'inline-block';
    console.log('Install button shown');
  }
  if (installBanner) {
    installBanner.style.display = 'block';
    console.log('Install banner shown');
  }
});

// Install button click handler
if (installButton) {
  installButton.addEventListener('click', async () => {
    console.log('Install button clicked');
    console.log('Deferred prompt available:', !!deferredPrompt);
    
    if (!deferredPrompt) {
      console.warn('No deferred prompt available. PWA may not be installable or already installed.');
      alert('This app is either already installed or cannot be installed on this device/browser.');
      return;
    }
    // Show the install prompt
    deferredPrompt.prompt();
    // Wait for the user to respond to the prompt
    const { outcome } = await deferredPrompt.userChoice;
    console.log(`User response to the install prompt: ${outcome}`);
    // Clear the deferred prompt variable
    deferredPrompt = null;
    // Hide the install button
    installButton.style.display = 'none';
    if (installBanner) {
      installBanner.style.display = 'none';
    }
  });
} else {
  console.warn('Install button element not found in DOM');
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
