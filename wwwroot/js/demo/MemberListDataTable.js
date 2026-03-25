$(document).ready(function () {

    let table = $('#ListDatatable').DataTable({
        order: [[0, 'asc']],
        info: false,
        processing: true,
        //colReorder: true,

        // IMPORTANT: FixedColumns needs horizontal scrolling enabled
        scrollX: true,
        //fixedColumns: {
        //    leftColumns: 1, // Note: In older versions use 'leftColumns' instead of 'start'
        //    rightColumns: 2 // Note: In older versions use 'rightColumns' instead of 'end'
        //},

        scrollCollapse: true,
        language: {
            processing: "處理中...",
            search: "",
            lengthMenu: "_MENU_",
            paginate: {
                first: "第一頁",
                last: "最後一頁",
                next: "下一頁",
                previous: "上一頁"
            },
            emptyTable: "目前沒有資料",
            zeroRecords: "沒有符合的資料",
            "searchPlaceholder": "請輸入查詢條件"
        }
    });
});
