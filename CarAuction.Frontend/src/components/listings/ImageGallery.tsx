import { useState } from 'react';
import { Image } from '@/types/common';
import { imagesApi } from '@/api/images';
import { Car, ZoomIn, X } from 'lucide-react';

export function ImageGallery({ images }: { images?: Image[] }) {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [isZoomed, setIsZoomed] = useState(false);

  if (!images || images.length === 0) {
    return (
      <div className="flex aspect-[16/10] w-full items-center justify-center rounded-2xl bg-card/60 border border-white/10 text-muted-foreground/30">
        <Car className="h-24 w-24" />
      </div>
    );
  }

  const selectedImage = images[selectedIndex];
  const selectedUrl = imagesApi.getImageUrl(selectedImage.id);

  return (
    <div className="space-y-3">
      {/* Main Image View */}
      <div className="relative aspect-[16/10] w-full overflow-hidden rounded-2xl border border-white/10 bg-black/40 group">
        <img
          src={selectedUrl}
          alt={selectedImage.fileName || 'Vehicle Image'}
          className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
        />

        <button
          type="button"
          onClick={() => setIsZoomed(true)}
          className="absolute bottom-3 right-3 flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-black/60 text-white text-xs font-semibold backdrop-blur-md hover:bg-black/80 transition-colors"
        >
          <ZoomIn className="h-3.5 w-3.5" />
          <span>Zoom</span>
        </button>
      </div>

      {/* Thumbnails */}
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.map((img, idx) => {
            const url = imagesApi.getImageUrl(img.id);
            const isSelected = idx === selectedIndex;
            return (
              <button
                key={img.id}
                type="button"
                onClick={() => setSelectedIndex(idx)}
                className={`relative h-16 w-24 flex-shrink-0 overflow-hidden rounded-xl border transition-all ${
                  isSelected
                    ? 'border-brand-500 ring-2 ring-brand-500/50 scale-95'
                    : 'border-white/10 opacity-70 hover:opacity-100'
                }`}
              >
                <img src={url} alt={`Thumbnail ${idx + 1}`} className="h-full w-full object-cover" />
              </button>
            );
          })}
        </div>
      )}

      {/* Zoom Modal */}
      {isZoomed && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/90 p-4 backdrop-blur-md animate-fade-in">
          <button
            onClick={() => setIsZoomed(false)}
            className="absolute top-4 right-4 p-2 rounded-xl bg-white/10 text-white hover:bg-white/20 transition-colors"
          >
            <X className="h-6 w-6" />
          </button>
          <img
            src={selectedUrl}
            alt="Full resolution vehicle preview"
            className="max-h-[90vh] max-w-[90vw] object-contain rounded-xl shadow-2xl"
          />
        </div>
      )}
    </div>
  );
}
