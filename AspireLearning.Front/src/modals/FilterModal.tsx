// src/components/FilterModal.tsx
import { useFilters } from "../context/FilterContext";

export default function FilterModal({ onClose }: { onClose: () => void }) {
    const { addFilter } = useFilters();

    // Burada toplu seçim yapılabilir (örnek için sadeleştirilmiş)
    const apply = () => {
        addFilter({ key: "status", value: "active" });
        addFilter({ key: "country", value: "Türkiye" });
        onClose();
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
            <div className="bg-white p-6 rounded">
                <h3 className="text-lg font-semibold mb-4">Add Filter</h3>
                <button onClick={apply} className="bg-blue-500 text-white px-4 py-2 rounded">Apply</button>
                <button onClick={onClose} className="ml-4 px-4 py-2">Cancel</button>
            </div>
        </div>
    );
}
