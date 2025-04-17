import { useState, useRef, useEffect } from 'react';
import { FiFilter, FiX } from 'react-icons/fi';

export interface FilterCriteria {
  field: string;
  value: string;
}

export interface FilterOption {
  value: string;
  label: string;
}

export interface FilterField {
  field: string;
  label: string;
  type?: 'text' | 'date' | 'select';
  options?: FilterOption[];
}

interface AddFilterDropdownProps {
  onApplyFilter: (filter: FilterCriteria) => void;
  availableFields: FilterField[];
  // Aktif filtrelerde hangi alanların olduğunu bildirecek prop
  activeFilterFields?: string[];
}

export default function AddFilterDropdown({ 
  onApplyFilter, 
  availableFields,
  activeFilterFields = []
}: AddFilterDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Filtreye eklenebilecek kullanılabilir alanlar (henüz eklenmemiş olanlar)
  const availableFieldsForFilter = availableFields.filter(field => 
    !activeFilterFields.includes(field.field)
  );

  const handleSelectField = (field: FilterField) => {
    // Seçilen alanı filtre olarak ekle, değer kısmını boş bırak
    onApplyFilter({
      field: field.field,
      value: ""  // Boş değer ile ekle, kullanıcı güncelleyecek
    });
    
    setIsOpen(false);
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <button 
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 px-3 py-2 border border-gray-300 rounded text-sm bg-white"
      >
        <FiFilter className="text-gray-600" />
        <span>Filtre Ekle</span>
      </button>

      {isOpen && (
        <div className="absolute z-30 mt-1 right-0 bg-white border rounded-md shadow-lg p-4 w-64">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-medium">Filtre Ekle</h3>
            <button 
              onClick={() => setIsOpen(false)} 
              className="text-gray-500 hover:text-gray-700"
            >
              <FiX className="w-4 h-4" />
            </button>
          </div>

          <div className="max-h-48 overflow-y-auto">
            {availableFieldsForFilter.length > 0 ? (
              <ul className="space-y-1">
                {availableFieldsForFilter.map(field => (
                  <li 
                    key={field.field}
                    className="px-3 py-2 hover:bg-gray-100 cursor-pointer text-sm rounded"
                    onClick={() => handleSelectField(field)}
                  >
                    {field.label}
                  </li>
                ))}
              </ul>
            ) : (
              <div className="text-sm text-gray-500 text-center p-2">
                Tüm filtreler eklenmiş
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
} 