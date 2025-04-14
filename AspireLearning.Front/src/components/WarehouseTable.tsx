import {useFilterContext} from "../context/FilterContext";
import {useState} from "react";
import {warehouses} from "../mock/warehouses";
import activeIcon from "../assets/active.png";
import inactiveIcon from "../assets/inactive.png";
import warningIcon from "../assets/warning.png";
import arrowIcon from "../assets/arrow.png";


export default function WarehouseTable({search}: { search: string }) {
    const [sortField, setSortField] = useState<string | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
    const {filterValues} = useFilterContext();

    const filteredData = warehouses.filter((item) => {
        const nameMatch = item.name.toLowerCase().includes(search.toLowerCase());

        const otherFiltersPass = (
            Object.entries(filterValues) as [keyof typeof filterValues, string[]][]
        ).every(
            ([key, selectedValues]) =>
                selectedValues.length === 0 || selectedValues.includes(item[key])
        );

        return nameMatch && otherFiltersPass;
    });

    const sortedData = [...filteredData].sort((a, b) => {
        if (!sortField) return 0;

        const valueA = a[sortField as keyof typeof a]?.toString().toLowerCase();
        const valueB = b[sortField as keyof typeof b]?.toString().toLowerCase();

        if (valueA < valueB) return sortDirection === "asc" ? -1 : 1;
        if (valueA > valueB) return sortDirection === "asc" ? 1 : -1;
        return 0;
    });

    const handleSort = (field: string) => {
        if (sortField === field) {
            setSortDirection((prev: "asc" | "desc") => (prev === "asc" ? "desc" : "asc"));
        } else {
            setSortField(field);
            setSortDirection("asc");
        }
    };

    const getStatusIcon = (status: string) => {
        const iconMap: Record<string, string> = {
            active: activeIcon,
            inactive: inactiveIcon,
            warning: warningIcon,
        };

        const iconSrc = iconMap[status];
        if (!iconSrc) return null;

        return (
            <img
                src={iconSrc}
                alt={status}
                className="w-[24px] h-[24px] object-contain"
                title={status}
            />
        );
    };

    return (
        <table className="w-full text-sm border-none">
            <thead className="bg-gray-100 text-left">
            <tr>
                <th
                    onClick={() => handleSort("name")}
                    className="py-3 px-4 border-b border-gray-333 cursor-pointer"
                >
                    Name
                    <img
                        src={arrowIcon}
                        alt="sort"
                        className={`w-[14px] h-[14px] ml-2 inline-block transition-transform duration-200  ${
                            sortDirection === "desc" ? "rotate-180" : ""
                        }`}/>

                </th>

                <th className="py-3 px-4 border-b border-gray-333">Country</th>
                <th className="py-3 px-4 border-b border-gray-333">City</th>
                <th className="py-3 px-4 border-b border-gray-333">Status</th>
                <th className="py-3 px-4 border-b border-gray-333">Category</th>
                <th
                    onClick={() => handleSort("lastActivity")}
                    className="py-3 px-4 border-b border-gray-333 cursor-pointer"
                >
                    Last Activity
                    <img
                        src={arrowIcon}
                        alt="sort"
                        className={`w-[14px] h-[14px] ml-2 inline-block transition-transform duration-200 ${
                            sortDirection === "desc" ? "rotate-180" : ""
                        }`}/>
                </th>
            </tr>
            </thead>
            <tbody>
            {sortedData.map((wh, i) => (
                <tr
                    key={i}
                    className="border-t hover:bg-gray-50 border-none border-separate border-spacing-y-[40px]"
                >
                    <td className="py-3 px-4 border-b border-gray-333 text-blue-link">
                        {wh.name}
                    </td>
                    <td className="py-3 px-4 border-b border-gray-333">{wh.country}</td>
                    <td className="py-3 px-4 border-b border-gray-333">{wh.city}</td>
                    <td className="py-3 px-4 border-b border-gray-333">
                        {getStatusIcon(wh.status)}
                    </td>
                    <td className="py-3 px-4 border-b border-gray-333">{wh.category}</td>
                    <td className="py-3 px-4 border-b border-gray-333">{wh.lastActivity}</td>
                </tr>
            ))}
            </tbody>
        </table>
    );
}
