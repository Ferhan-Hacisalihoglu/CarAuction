import { Search, X } from 'lucide-react';
import { Input } from '@/components/ui/Input';

export function SearchInput({
  value,
  onChange,
  placeholder = 'Search vehicles, auctions...',
  className,
}: {
  value: string;
  onChange: (val: string) => void;
  placeholder?: string;
  className?: string;
}) {
  return (
    <Input
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      className={className}
      icon={<Search className="h-4 w-4 text-muted-foreground" />}
      rightIcon={
        value ? (
          <button
            type="button"
            onClick={() => onChange('')}
            className="hover:text-foreground transition-colors p-1"
          >
            <X className="h-4 w-4" />
          </button>
        ) : undefined
      }
    />
  );
}
