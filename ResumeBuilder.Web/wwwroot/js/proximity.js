/**
 * Proximity responsiveness for buttons (magnetic pull + dynamic box shadow glow)
 * Triggered when cursor moves within range.
 */
document.addEventListener('mousemove', (e) => {
    const buttons = document.querySelectorAll('.btn-primary, .btn-outline-light, .btn-outline-primary, .hover-lift');
    
    buttons.forEach(btn => {
        const rect = btn.getBoundingClientRect();
        
        // Button center coordinate
        const btnX = rect.left + rect.width / 2;
        const btnY = rect.top + rect.height / 2;
        
        const distX = e.clientX - btnX;
        const distY = e.clientY - btnY;
        const distance = Math.sqrt(distX * distX + distY * distY);
        
        // Define proximity range (e.g. 100px)
        const range = 100;
        
        if (distance < range) {
            // Calculate pull factor (higher proximity = stronger pull)
            const proximityFactor = (1 - (distance / range)); // 0 to 1
            
            // Magnetic pull translation (max 4px pull for subtle premium feel)
            const pullX = (distX / distance) * proximityFactor * 4;
            const pullY = (distY / distance) * proximityFactor * 4;
            
            // Apply magnetic translation and micro-scale up
            btn.style.transform = `translate(${pullX}px, ${pullY}px) scale(${1.0 + (proximityFactor * 0.02)})`;
            
            // Proximity shadow glow color (based on active theme brand color)
            const isDark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
            const shadowColor = isDark ? 'rgba(148, 133, 230, 0.45)' : 'rgba(94, 75, 182, 0.3)';
            
            btn.style.boxShadow = `0 10px 25px -5px ${shadowColor}, 0 0 20px ${proximityFactor * 12}px ${shadowColor}`;
            btn.style.transition = 'transform 0.15s cubic-bezier(0.25, 0.46, 0.45, 0.94), box-shadow 0.15s ease-out';
        } else {
            // Restore standard states smoothly
            btn.style.transform = '';
            btn.style.boxShadow = '';
            btn.style.transition = 'transform 0.3s ease, box-shadow 0.3s ease';
        }
    });
});

// Reset positions when mouse leaves document boundary
document.addEventListener('mouseleave', () => {
    const buttons = document.querySelectorAll('.btn-primary, .btn-outline-light, .btn-outline-primary, .hover-lift');
    buttons.forEach(btn => {
        btn.style.transform = '';
        btn.style.boxShadow = '';
    });
});
