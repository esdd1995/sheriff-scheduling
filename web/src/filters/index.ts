import Vue from 'vue'


Vue.filter('beautify-date', function(date){
	enum MonthList {'Jan' = 1, 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'}
	if(date)
		return date.substr(8,2) + ' ' + MonthList[Number(date.substr(5,2))] + ' ' + date.substr(0,4);
	else
		return ''
})

Vue.filter('capitilize', function(str: string){
	
	if(str)
		return str.charAt(0).toUpperCase() + (str.slice(1)).toLowerCase();
	else
		return ''
	
})