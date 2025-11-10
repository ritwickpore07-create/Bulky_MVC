
let dataTable;

$(function () {
	var url = window.location.search;
	if (url.includes("inprocess")) {
		loadDataTable("inprocess");
	}
	else {
		if (url.includes("pending")) {
			loadDataTable("pending");
		}
		else {
			if (url.includes("completed")) {
				loadDataTable("completed");
			}
			else {
				if (url.includes("approved")) {
					loadDataTable("approved");
				}
				else {
					if (url.includes("cancelled")) {
						loadDataTable("cancelled");
					}
					else
					{
						loadDataTable("all");
					}
				}
			}
		}
	}
});

function loadDataTable(status) {
	dataTable = $('#tblData').DataTable({
		"ajax": { url: '/admin/order/getall?status=' + status },
		"columns": [
			{ data: 'id', "width": "5%", className: 'text-center' },
			{ data: 'name', "width": "20%", className: 'text-center' },
			{ data: 'phoneNumber', "width": "15%", className: 'text-center' },
			{ data: 'applicationUser.email', "width": "20%", className: 'text-center' },
			{ data: 'orderStatus', "width": "10%", className: 'text-center' },
			{ data: 'orderTotal', "width": "10%", className: 'text-center' },
			{
				data: 'id',
				width: "20%",
				className: 'text-center',
				render: function (data) {
					return `<div class="btn-group w-75" role="group">
						    <a href = "/admin/order/details?orderId=${data}" class="btn btn-success mx-2" >
							<i class="bi bi-pencil"></i> </a >							
							</div>`
				},
			},
		]
	});
}





