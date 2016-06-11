function createMap(mapId) {
    var map;
    var trafficLayer;
    map = new google.maps.Map(document.getElementById(mapId), {
        center: { lat: 47.616313, lng: -122.301881 },
        zoom: 12,
        disableDefaultUI: true,
        draggable: false,
        scrollwheel: false,
        styles:
          [{
              "featureType": "all",
              "elementType": "labels",
              "stylers": [{
                  "visibility": "off"
              }]
          }, {
              "featureType": "administrative",
              "elementType": "geometry.fill",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 80
              }]
          }, {
              "featureType": "administrative",
              "elementType": "geometry.stroke",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 83
              }, {
                  "weight": 1.2
              }]
          }, {
              "featureType": "landscape",
              "elementType": "geometry",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 80
              }]
          }, {
              "featureType": "poi",
              "elementType": "geometry",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 79
              }]
          }, {
              "featureType": "road.highway",
              "elementType": "geometry.fill",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 83
              }]
          }, {
              "featureType": "road.highway",
              "elementType": "geometry.stroke",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 71
              }, {
                  "weight": 0.2
              }]
          }, {
              "featureType": "road.arterial",
              "elementType": "geometry",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 82
              }]
          }, {
              "featureType": "road.local",
              "elementType": "geometry",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 84
              }]
          }, {
              "featureType": "transit",
              "elementType": "geometry",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 81
              }]
          }, {
              "featureType": "water",
              "elementType": "geometry",
              "stylers": [{
                  "color": "#000000"
              }, {
                  "lightness": 83
              }]
          }]
    });

    trafficLayer = new google.maps.TrafficLayer();
    trafficLayer.setMap(map);
}