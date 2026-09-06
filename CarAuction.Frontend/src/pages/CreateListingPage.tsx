import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { useCreateListing } from '@/hooks/useListings';
import { imagesApi } from '@/api/images';
import { listingSchema } from '@/utils/validators';
import { CreateListingRequest } from '@/types/listing';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Button } from '@/components/ui/Button';
import { Label } from '@/components/ui/Label';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/Card';
import { toast } from '@/hooks/useToast';
import { Car, Gavel, Upload, Image as ImageIcon, X } from 'lucide-react';

export function CreateListingPage() {
  const navigate = useNavigate();
  const createListingMutation = useCreateListing();
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [isUploading, setIsUploading] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<CreateListingRequest>({
    resolver: zodResolver(listingSchema),
    defaultValues: {
      isAuction: false,
      minBidIncrement: 50,
      price: 1000,
    },
  });

  const isAuction = watch('isAuction');

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const filesArr = Array.from(e.target.files);
      setSelectedFiles((prev) => [...prev, ...filesArr]);
    }
  };

  const removeFile = (index: number) => {
    setSelectedFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const onSubmit = async (data: CreateListingRequest) => {
    setIsUploading(true);
    try {
      const newListing = await createListingMutation.mutateAsync(data);

      // Upload selected images to newly created listing
      if (selectedFiles.length > 0) {
        for (const file of selectedFiles) {
          try {
            await imagesApi.upload(newListing.id, file);
          } catch (imgErr) {
            console.warn('Failed to upload image file:', imgErr);
          }
        }
      }

      toast.success('Your vehicle listing has been published!', 'Listing Created');
      navigate(`/listings/${newListing.id}`);
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to create listing. Please verify inputs.';
      toast.error(msg, 'Creation Error');
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="container mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
          <Car className="h-4 w-4" />
          <span>Vehicle Submission</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          List a Vehicle for Sale
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Publish a vehicle as a fixed-price sale or launch an automated live auction.
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* Core Specs Card */}
        <Card className="glass-panel border-white/10 p-6 space-y-6">
          <CardHeader className="p-0 pb-4 border-b border-white/5">
            <CardTitle className="text-lg">Vehicle Details</CardTitle>
            <CardDescription>Enter the title, description, and primary asking price</CardDescription>
          </CardHeader>

          <CardContent className="p-0 space-y-4">
            <div className="space-y-1">
              <Label htmlFor="title">Vehicle Title / Make & Model</Label>
              <Input
                id="title"
                placeholder="e.g. 2022 Porsche 911 GT3 Touring"
                {...register('title')}
              />
              {errors.title && <p className="text-xs text-rose-400 mt-1">{errors.title.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="description">Comprehensive Description & History</Label>
              <Textarea
                id="description"
                rows={5}
                placeholder="Include mileage, condition, paint code, options, service history, clean title status..."
                {...register('description')}
              />
              {errors.description && (
                <p className="text-xs text-rose-400 mt-1">{errors.description.message}</p>
              )}
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label htmlFor="price">Asking / Reserve Price (USD)</Label>
                <Input
                  id="price"
                  type="number"
                  placeholder="e.g. 185000"
                  {...register('price')}
                />
                {errors.price && <p className="text-xs text-rose-400 mt-1">{errors.price.message}</p>}
              </div>

              {/* Auction Toggle Option */}
              <div className="space-y-1 flex flex-col justify-end">
                <label className="flex items-center gap-3 p-3 rounded-xl border border-white/10 bg-secondary/40 cursor-pointer hover:bg-secondary/60 transition-colors">
                  <input
                    type="checkbox"
                    className="h-4 w-4 rounded border-border text-brand-600 focus:ring-brand-500"
                    {...register('isAuction')}
                  />
                  <div>
                    <span className="text-sm font-bold text-foreground flex items-center gap-1.5">
                      <Gavel className="h-4 w-4 text-amber-400" />
                      <span>Launch as Live Auction</span>
                    </span>
                    <span className="text-xs text-muted-foreground block">
                      Enable real-time bidding with anti-snipe countdown
                    </span>
                  </div>
                </label>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Conditional Auction Fields */}
        {isAuction && (
          <Card className="glass-panel border-amber-500/20 p-6 space-y-6 bg-amber-500/5">
            <CardHeader className="p-0 pb-4 border-b border-amber-500/10">
              <CardTitle className="text-lg text-amber-300 flex items-center gap-2">
                <Gavel className="h-5 w-5" />
                <span>Live Auction Configuration</span>
              </CardTitle>
              <CardDescription>
                Configure starting price, auction schedule, and minimum bid increments
              </CardDescription>
            </CardHeader>

            <CardContent className="p-0 grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label htmlFor="startingPrice">Starting Bid (USD)</Label>
                <Input
                  id="startingPrice"
                  type="number"
                  placeholder="e.g. 50000"
                  {...register('startingPrice')}
                />
              </div>

              <div className="space-y-1">
                <Label htmlFor="minBidIncrement">Minimum Bid Increment (USD)</Label>
                <Input
                  id="minBidIncrement"
                  type="number"
                  placeholder="e.g. 250"
                  {...register('minBidIncrement')}
                />
              </div>

              <div className="space-y-1">
                <Label htmlFor="startTime">Start Date & Time</Label>
                <Input
                  id="startTime"
                  type="datetime-local"
                  {...register('startTime')}
                />
              </div>

              <div className="space-y-1">
                <Label htmlFor="endTime">End Date & Time</Label>
                <Input
                  id="endTime"
                  type="datetime-local"
                  {...register('endTime')}
                />
              </div>
            </CardContent>
          </Card>
        )}

        {/* Image Upload Box */}
        <Card className="glass-panel border-white/10 p-6 space-y-4">
          <CardHeader className="p-0 pb-2">
            <CardTitle className="text-lg">Vehicle Photography</CardTitle>
            <CardDescription>Upload high resolution photos of exterior, interior, and engine bay</CardDescription>
          </CardHeader>

          <CardContent className="p-0 space-y-4">
            <div className="relative border-2 border-dashed border-white/15 rounded-2xl p-8 text-center hover:border-brand-500/50 transition-colors bg-secondary/20">
              <input
                type="file"
                multiple
                accept="image/*"
                onChange={handleFileChange}
                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
              />
              <div className="space-y-2 pointer-events-none">
                <Upload className="h-8 w-8 mx-auto text-brand-400" />
                <p className="text-sm font-semibold text-foreground">
                  Drop photos here or click to browse
                </p>
                <p className="text-xs text-muted-foreground">PNG, JPG, WebP up to 10MB each</p>
              </div>
            </div>

            {/* Selected File Previews */}
            {selectedFiles.length > 0 && (
              <div className="flex flex-wrap gap-3 pt-2">
                {selectedFiles.map((file, idx) => (
                  <div
                    key={idx}
                    className="relative flex items-center gap-2 px-3 py-1.5 rounded-xl bg-secondary border border-border text-xs font-medium"
                  >
                    <ImageIcon className="h-4 w-4 text-muted-foreground" />
                    <span className="max-w-[150px] truncate">{file.name}</span>
                    <button
                      type="button"
                      onClick={() => removeFile(idx)}
                      className="text-muted-foreground hover:text-rose-400 p-0.5"
                    >
                      <X className="h-3.5 w-3.5" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Submit */}
        <div className="flex justify-end gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(-1)}
            disabled={isUploading}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            variant="primary"
            size="lg"
            isLoading={isUploading || createListingMutation.isPending}
          >
            Publish Listing
          </Button>
        </div>
      </form>
    </div>
  );
}
