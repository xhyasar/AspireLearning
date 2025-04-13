import { useFilterContext } from "../context/FilterContext";
import { warehouses } from "../mock/warehouses";

export default function WarehouseTable({ search }: { search: string }) {
    const { filterValues } = useFilterContext();

    const filteredData = warehouses.filter((item) => {
        const nameMatch = item.name.toLowerCase().includes(search.toLowerCase());

        const otherFiltersPass = (Object.entries(filterValues) as [keyof typeof filterValues, string[]][]).every(
            ([key, selectedValues]) =>
                selectedValues.length === 0 || selectedValues.includes(item[key])
        );

        return nameMatch && otherFiltersPass;
    });

    return (
        <table className="w-full text-sm border">
            <thead className="bg-gray-100 text-left">
            <tr>
                <th className="px-3 py-2">Name</th>
                <th className="px-3 py-2">Country</th>
                <th className="px-3 py-2">City</th>
                <th className="px-3 py-2">Status</th>
                <th className="px-3 py-2">Category</th>
                <th className="px-3 py-2">Last Activity</th>
            </tr>
            </thead>
            <tbody>
            {filteredData.map((wh, i) => (
                <tr key={i} className="border-t hover:bg-gray-50">
                    <td className="px-3 py-2">{wh.name}</td>
                    <td className="px-3 py-2">{wh.country}</td>
                    <td className="px-3 py-2">{wh.city}</td>
                    <td className="px-3 py-2">{wh.status}</td>
                    <td className="px-3 py-2">{wh.category}</td>
                    <td className="px-3 py-2">{wh.lastActivity}</td>
                </tr>
            ))}
            </tbody>
        </table>
    );
}
