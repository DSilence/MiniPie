<template>
    <div id="cover-image">
        <img :src="coverImage"/>
    </div>
</template>
<script>
    export default {
      name: 'cover-image',
      props: {coverImage}
    }
</script>