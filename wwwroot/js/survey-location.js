/* Survey Location Page JavaScript */

let map;
let marker;
let geocoder;
const MAPBOX_TOKEN = 'pk.eyJ1Ijoic3VkaGFuc3VtYXBib3giLCJhIjoiY21pZTh0ZjI2MDA2bTNmc2d3ZGJqMjhsMSJ9.P0jMewzu7PpPhL0LcikXgg';

function initializeMap() {
    mapboxgl.accessToken = MAPBOX_TOKEN;
    
    // Get initial coordinates from form
    const latInput = document.querySelector('input[name="LocLat"]');
    const lngInput = document.querySelector('input[name="LocLog"]');
    const hasExistingCoords = latInput.value && lngInput.value;
    const initialLat = hasExistingCoords ? parseFloat(latInput.value) : 20.5937;
    const initialLng = hasExistingCoords ? parseFloat(lngInput.value) : 78.9629;
    
    // Initialize map
    map = new mapboxgl.Map({
        container: 'mapContainer',
        style: 'mapbox://styles/mapbox/streets-v12',
        center: [initialLng, initialLat],
        zoom: 12
    });
    
    // Add geocoder (search box)
    geocoder = new MapboxGeocoder({
        accessToken: mapboxgl.accessToken,
        mapboxgl: mapboxgl,
        marker: false,
        placeholder: 'Search for a location...',
        proximity: {
            longitude: initialLng,
            latitude: initialLat
        }
    });
    
    map.addControl(geocoder, 'top-right');
    
    // When a location is selected from search
    geocoder.on('result', function(e) {
        const coords = e.result.geometry.coordinates;
        marker.setLngLat(coords);
        updateCoordinates(coords[1], coords[0]);
        
        // Optionally update location name if field exists
        const locationNameInput = document.querySelector('input[name="LocName"]');
        if (locationNameInput && !locationNameInput.value) {
            locationNameInput.value = e.result.place_name.split(',')[0];
        }
    });
    
    // Add navigation controls
    map.addControl(new mapboxgl.NavigationControl());
    
    // Add fullscreen control
    map.addControl(new mapboxgl.FullscreenControl());
    
    // Create draggable marker
    marker = new mapboxgl.Marker({
        draggable: true,
        color: '#FF0000'
    })
    .setLngLat([initialLng, initialLat])
    .addTo(map);
    
    // Update coordinates when marker is dragged
    marker.on('dragend', function() {
        const lngLat = marker.getLngLat();
        updateCoordinates(lngLat.lat, lngLat.lng);
    });
    
    // Update marker when clicking on map
    map.on('click', function(e) {
        marker.setLngLat(e.lngLat);
        updateCoordinates(e.lngLat.lat, e.lngLat.lng);
    });
    
    // If no existing coordinates, try to get user's current location
    if (!hasExistingCoords && navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function(position) {
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;
                
                // Update map, marker and inputs
                map.flyTo({
                    center: [lng, lat],
                    zoom: 15,
                    essential: true
                });
                marker.setLngLat([lng, lat]);
                updateCoordinates(lat, lng);
            },
            function(error) {
                // Silently fail - user can manually select or use GPS button
                console.log('Auto-location detection failed:', error.message);
            },
            {
                enableHighAccuracy: true,
                timeout: 5000,
                maximumAge: 0
            }
        );
    }
}

function updateCoordinates(lat, lng) {
    document.querySelector('input[name="LocLat"]').value = lat.toFixed(6);
    document.querySelector('input[name="LocLog"]').value = lng.toFixed(6);
}

function fetchLocation() {
    // Check if geolocation is supported
    if (!navigator.geolocation) {
        alert('Geolocation is not supported by this browser.');
        return;
    }

    // Check if the page is served over HTTPS (or localhost)
    const isSecureContext = window.isSecureContext || location.protocol === 'https:' || location.hostname === 'localhost';
    
    if (!isSecureContext) {
        alert('Geolocation requires HTTPS connection. Please:\n\n1. Enable HTTPS on your server, or\n2. Enter coordinates manually, or\n3. Access via localhost for testing.\n\nCurrent protocol: ' + location.protocol);
        return;
    }

    // Show loading state
    const fetchBtn = document.getElementById('fetchLocationBtn');
    const originalText = fetchBtn ? fetchBtn.innerHTML : '';
    if (fetchBtn) {
        fetchBtn.disabled = true;
        fetchBtn.innerHTML = '<i class="bi bi-arrow-clockwise spinner-border spinner-border-sm me-1"></i>Fetching...';
    }

    navigator.geolocation.getCurrentPosition(
        function (position) {
            // Success callback
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;
            
            updateCoordinates(lat, lng);
            
            // Update map and marker
            if (map && marker) {
                map.flyTo({
                    center: [lng, lat],
                    zoom: 15,
                    essential: true
                });
                marker.setLngLat([lng, lat]);
            }
            
            // Reset button state
            if (fetchBtn) {
                fetchBtn.disabled = false;
                fetchBtn.innerHTML = originalText;
            }
            
            // Show success message
            alert('Location fetched successfully!\nLatitude: ' + lat.toFixed(6) + '\nLongitude: ' + lng.toFixed(6));
        },
        function (error) {
            // Error callback
            let errorMessage = 'Unable to fetch location: ';
            
            switch(error.code) {
                case error.PERMISSION_DENIED:
                    errorMessage += 'User denied the request for Geolocation. Please allow location access in your browser settings.';
                    break;
                case error.POSITION_UNAVAILABLE:
                    errorMessage += 'Location information is unavailable. Please check your device GPS settings.';
                    break;
                case error.TIMEOUT:
                    errorMessage += 'The request to get user location timed out. Please try again.';
                    break;
                default:
                    errorMessage += error.message;
                    break;
            }
            
            alert(errorMessage);
            
            // Reset button state
            if (fetchBtn) {
                fetchBtn.disabled = false;
                fetchBtn.innerHTML = originalText;
            }
        },
        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        }
    );
}

function toggleWayType() {
    const locationTypeSelect = document.getElementById('LocationType');
    const wayTypeContainer = document.getElementById('wayTypeContainer');
    const wayTypeSelect = document.getElementById('WayType');
    
    if (locationTypeSelect && wayTypeContainer && wayTypeSelect) {
        if (locationTypeSelect.value === 'Traffic') {
            wayTypeContainer.style.display = 'block';
            wayTypeSelect.setAttribute('required', 'required');
        } else {
            wayTypeContainer.style.display = 'none';
            wayTypeSelect.removeAttribute('required');
            wayTypeSelect.value = ''; // Clear selection
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    // Initialize MapBox
    initializeMap();
    
    // Fetch location button
    var btn = document.getElementById('fetchLocationBtn');
    if (btn) {
        btn.addEventListener('click', fetchLocation);
    }
    
    // Update marker when coordinates are manually changed
    const latInput = document.querySelector('input[name="LocLat"]');
    const lngInput = document.querySelector('input[name="LocLog"]');
    
    if (latInput && lngInput) {
        latInput.addEventListener('change', function() {
            const lat = parseFloat(this.value);
            const lng = parseFloat(lngInput.value);
            if (!isNaN(lat) && !isNaN(lng) && marker && map) {
                marker.setLngLat([lng, lat]);
                map.flyTo({ center: [lng, lat], zoom: 12 });
            }
        });
        
        lngInput.addEventListener('change', function() {
            const lat = parseFloat(latInput.value);
            const lng = parseFloat(this.value);
            if (!isNaN(lat) && !isNaN(lng) && marker && map) {
                marker.setLngLat([lng, lat]);
                map.flyTo({ center: [lng, lat], zoom: 12 });
            }
        });
    }
    
    // Show/Hide Way Type based on Location Type
    const locationTypeSelect = document.getElementById('LocationType');
    
    // Initialize on page load
    toggleWayType();
    
    // Add event listener for changes
    if (locationTypeSelect) {
        locationTypeSelect.addEventListener('change', toggleWayType);
    }
});
