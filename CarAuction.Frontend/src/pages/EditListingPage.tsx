import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useParams, useNavigate } from 'react-router-dom';
import { useListing, useUpdateListing } from '@/hooks/useListings';
import { imagesApi } from '@/api/images';
import { z } from 'zod';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Button } from '@/components/ui/Button';
import { Label } from '@/components/ui/Label';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { toast } from '@/hooks/useToast';
import { Edit, Upload, Trash2, ArrowLeft } from 'lucide-react';

const editSchema = z.object({
  title: z.string().min(3, 'Title must be at least 3 characters'),
  description: z.string().min(10, 'Description must be at least 10 characters'),
  price: z.coerce.number().positive('Price must be greater than 0'),
});

type EditFormData = z.infer<typeof editSchema>;

export function EditListingPage() {
  const { id } = useParams<{ id: string }>();
  const listingId = Number(id);
  const navigate = useNavigate();

  const { data: listing, isLoading } = useListing(listingId);
  const updateListingMutation = useUpdateListing();

  const [isUploading, setIsUploading] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<EditFormData>({
    resolver: zodResolver(editSchema),
  });

  useEffect(() => {
    if (listing) {
      reset({
        title: listing.title,
        description: listing.description,
        price: listing.price,
      });
    }
  }, [listing, reset]);

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setIsUploading(true);
      try {
        for (const file of Array.from(e.target.files)) {
          await imagesApi.upload(listingId, file);
        }
        toast.success('Images uploaded successfully');
        window.location.reload();
      } catch (err) {
        toast.error('Failed to upload image');
      } finally {
        setIsUploading(false);
      }
    }
  };

  const handleImageDelete = async (imageId: number) => {
    try {
      await imagesApi.delete(imageId);
      toast.success('Image deleted');
      window.location.reload();
    } catch {
      toast.error('Failed to delete image');
    }
  };

  const onSubmit = async (data: EditFormData) => {
    try {
      await updateListingMutation.mutateAsync({ id: listingId, data });
      toast.success('Listing updated successfully!', 'Vehicle Saved');
      navigate(`/listings/${listingId}`);
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to update listing', 'Update Error');
    }
  };

  if (isLoading) {
    return <LoadingSpinner size="lg" message="Loading vehicle data..." className="min-h-[50vh]" />;
  }

  return (
    <div className="container mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate(-1)}
          className="inline-flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back</span>
        </button>
      </div>

      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
          <Edit className="h-4 w-4" />
          <span>Editing Listing #{listingId}</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          Update Vehicle Listing
        </h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        <Card className="glass-panel border-white/10 p-6 space-y-4">
          <div className="space-y-1">
            <Label htmlFor="title">Vehicle Title</Label>
            <Input id="title" {...register('title')} />
            {errors.title && <p className="text-xs text-rose-400 mt-1">{errors.title.message}</p>}
          </div>

          <div className="space-y-1">
            <Label htmlFor="description">Vehicle Description</Label>
            <Textarea id="description" rows={6} {...register('description')} />
            {errors.description && (
              <p className="text-xs text-rose-400 mt-1">{errors.description.message}</p>
            )}
          </div>

          <div className="space-y-1 max-w-xs">
            <Label htmlFor="price">Asking Price (USD)</Label>
            <Input id="price" type="number" {...register('price')} />
            {errors.price && <p className="text-xs text-rose-400 mt-1">{errors.price.message}</p>}
          </div>
        </Card>

        {/* Existing Images Management */}
        <Card className="glass-panel border-white/10 p-6 space-y-4">
          <CardHeader className="p-0 pb-2">
            <CardTitle className="text-base">Manage Images</CardTitle>
          </CardHeader>
          <CardContent className="p-0 space-y-4">
            {listing?.images && listing.images.length > 0 && (
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                {listing.images.map((img) => (
                  <div key={img.id} className="relative aspect-[16/10] rounded-xl overflow-hidden border border-white/10 group">
                    <img
                      src={imagesApi.getImageUrl(img.id)}
                      alt="Vehicle image"
                      className="h-full w-full object-cover"
                    />
                    <button
                      type="button"
                      onClick={() => handleImageDelete(img.id)}
                      className="absolute top-2 right-2 p-1.5 rounded-lg bg-rose-600/90 text-white opacity-0 group-hover:opacity-100 transition-opacity"
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                    </button>
                  </div>
                ))}
              </div>
            )}

            <div>
              <label className="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-secondary hover:bg-accent text-xs font-semibold cursor-pointer border border-border transition-colors">
                <Upload className="h-4 w-4 text-brand-400" />
                <span>Upload New Photos</span>
                <input
                  type="file"
                  multiple
                  accept="image/*"
                  onChange={handleImageUpload}
                  className="hidden"
                  disabled={isUploading}
                />
              </label>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" onClick={() => navigate(-1)}>
            Cancel
          </Button>
          <Button type="submit" variant="primary" isLoading={updateListingMutation.isPending}>
            Save Changes
          </Button>
        </div>
      </form>
    </div>
  );
}
